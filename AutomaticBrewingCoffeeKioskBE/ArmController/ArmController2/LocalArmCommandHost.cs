using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ArmController2
{
    internal sealed class ArmDeviceCommandRequest
    {
        public string CommandId { get; set; }
        public int SchemaVersion { get; set; }
        public string CorrelationId { get; set; }
        public string WorkflowId { get; set; }
        public string StepId { get; set; }
        public string DeviceId { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public int TimeoutMs { get; set; }
    }

    internal sealed class ArmDeviceCommandResult
    {
        public string CommandId { get; set; }
        public int SchemaVersion { get; set; }
        public string CorrelationId { get; set; }
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> Payload { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CompletedAtUtc { get; set; }
    }

    internal sealed class LocalArmCommandHost : IDisposable
    {
        private readonly string deviceId;
        private readonly Func<ArmDeviceCommandRequest, ArmDeviceCommandResult> handler;
        private readonly ArmCommandJournal journal;
        private IConnection connection;
        private IModel channel;

        public LocalArmCommandHost(
            string deviceId,
            Func<ArmDeviceCommandRequest, ArmDeviceCommandResult> handler,
            string journalPath)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("DEVICE_ID is required for local Arm ingress.", "deviceId");

            this.deviceId = deviceId;
            this.handler = handler ?? throw new ArgumentNullException("handler");
            journal = new ArmCommandJournal(journalPath);
        }

        public void Start()
        {
            journal.Initialize();

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = ParsePort(),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND, ExchangeType.Direct, durable: true, autoDelete: false);
            channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx", ExchangeType.Direct, durable: true, autoDelete: false);
            channel.QueueDeclare(QueueConstants.QUEUE_DEVICE_COMMAND_DLQ, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(
                QueueConstants.QUEUE_DEVICE_COMMAND_DLQ,
                QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
                QueueConstants.ROUTING_DEVICE_COMMAND);

            var queueName = "device-command." + Sanitize(deviceId);
            var arguments = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
                ["x-dead-letter-routing-key"] = QueueConstants.ROUTING_DEVICE_COMMAND
            };
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            channel.QueueBind(queueName, QueueConstants.EXCHANGE_DEVICE_COMMAND, QueueConstants.ROUTING_DEVICE_COMMAND);
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (sender, delivery) => await HandleDeliveryAsync(delivery);
            channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Local RabbitMQ Arm controller listening: device=" + deviceId + " queue=" + queueName);
        }

        private async Task HandleDeliveryAsync(BasicDeliverEventArgs delivery)
        {
            ArmDeviceCommandRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<ArmDeviceCommandRequest>(
                    Encoding.UTF8.GetString(delivery.Body.ToArray())) ?? throw new InvalidDataException("Empty device command.");
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Rejected malformed Arm command: " + error.Message);
                channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (!string.Equals(request.DeviceId, deviceId, StringComparison.Ordinal))
            {
                channel.BasicAck(delivery.DeliveryTag, multiple: false);
                return;
            }

            ArmDeviceCommandResult result;
            try
            {
                var replay = journal.TryClaim(request);
                result = replay == null ? handler(request) : JsonConvert.DeserializeObject<ArmDeviceCommandResult>(replay);
                if (replay == null)
                    journal.SaveResult(request, result);
            }
            catch (InvalidOperationException error) when (
                error.Message != null &&
                error.Message.IndexOf("uncertain outcome", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = Failure(request, "DEVICE_UNKNOWN_OUTCOME", error.Message);
            }
            catch (Exception error)
            {
                result = Failure(request, "DEVICE_HANDLER_FAILURE", error.Message);
                journal.SaveResult(request, result);
            }

            try
            {
                var replyTo = delivery.BasicProperties == null ? null : delivery.BasicProperties.ReplyTo;
                if (!string.IsNullOrWhiteSpace(replyTo))
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
                    properties.CorrelationId = request.CommandId;
                    channel.BasicPublish(
                        exchange: string.Empty,
                        routingKey: replyTo,
                        mandatory: false,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
                }
            }
            finally
            {
                channel.BasicAck(delivery.DeliveryTag, multiple: false);
            }

            await Task.CompletedTask;
        }

        private static ArmDeviceCommandResult Failure(ArmDeviceCommandRequest request, string code, string message)
        {
            return new ArmDeviceCommandResult
            {
                CommandId = request.CommandId,
                SchemaVersion = request.SchemaVersion,
                CorrelationId = request.CorrelationId,
                DeviceId = request.DeviceId,
                Status = "Failed",
                Payload = new Dictionary<string, string>(),
                ErrorCode = code,
                ErrorMessage = message == null ? null : message.Substring(0, Math.Min(message.Length, 2000)),
                CompletedAtUtc = DateTime.UtcNow
            };
        }

        private static int ParsePort()
        {
            int port;
            return int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out port) ? port : 5672;
        }

        private static string Sanitize(string value)
        {
            return new string(value.Select(character =>
                char.IsLetterOrDigit(character) || character == '-' || character == '_' ? character : '_').ToArray());
        }

        public void Dispose()
        {
            if (channel != null) channel.Dispose();
            if (connection != null) connection.Dispose();
            journal.Dispose();
        }
    }

    internal sealed class ArmCommandJournal : IDisposable
    {
        private readonly object sync = new object();
        private readonly string path;
        private Dictionary<string, ArmCommandRecord> records;

        public ArmCommandJournal(string path)
        {
            this.path = Path.GetFullPath(path);
            records = new Dictionary<string, ArmCommandRecord>(StringComparer.Ordinal);
        }

        public void Initialize()
        {
            lock (sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                records = Load();
                var changed = false;
                foreach (var record in records.Values)
                {
                    if (record.Status == "Executing")
                    {
                        record.Status = "Unknown";
                        record.UpdatedAtUtc = DateTime.UtcNow;
                        changed = true;
                    }
                }
                if (changed || !File.Exists(path)) Persist();
            }
        }

        public string TryClaim(ArmDeviceCommandRequest request)
        {
            lock (sync)
            {
                ArmCommandRecord record;
                if (!records.TryGetValue(request.CommandId, out record))
                {
                    record = new ArmCommandRecord
                    {
                        CommandId = request.CommandId,
                        CorrelationId = request.CorrelationId,
                        DeviceId = request.DeviceId,
                        Status = "Received",
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    records[request.CommandId] = record;
                }

                if (record.Status == "Completed" || record.Status == "Failed")
                    return record.ResultJson;
                if (record.Status == "Unknown")
                    throw new InvalidOperationException("Command has an uncertain outcome and requires explicit reconciliation.");

                record.Status = "Executing";
                record.UpdatedAtUtc = DateTime.UtcNow;
                Persist();
                return null;
            }
        }

        public void SaveResult(ArmDeviceCommandRequest request, ArmDeviceCommandResult result)
        {
            lock (sync)
            {
                ArmCommandRecord record;
                if (!records.TryGetValue(request.CommandId, out record))
                    throw new InvalidOperationException("Command is missing from the Arm journal.");
                record.Status = result.Status;
                record.ResultJson = JsonConvert.SerializeObject(result);
                record.UpdatedAtUtc = DateTime.UtcNow;
                Persist();
            }
        }

        public void Reconcile(string commandId, string resolution)
        {
            lock (sync)
            {
                if (resolution != "Completed" && resolution != "Failed")
                    throw new ArgumentException("Resolution must be Completed or Failed.", "resolution");

                ArmCommandRecord record;
                if (!records.TryGetValue(commandId, out record))
                    throw new InvalidOperationException("Command was not found in the Arm journal: " + commandId);
                if (record.Status != "Unknown")
                    throw new InvalidOperationException("Only Unknown Arm commands can be reconciled.");

                var result = new ArmDeviceCommandResult
                {
                    CommandId = record.CommandId,
                    SchemaVersion = 1,
                    CorrelationId = record.CorrelationId,
                    DeviceId = record.DeviceId,
                    Status = resolution,
                    Payload = new Dictionary<string, string> { ["operator"] = "manual-reconciliation" },
                    ErrorCode = resolution == "Failed" ? "OPERATOR_RECONCILED" : null,
                    ErrorMessage = resolution == "Failed" ? "Physical outcome reconciled by operator." : null,
                    CompletedAtUtc = DateTime.UtcNow
                };
                record.Status = resolution;
                record.ResultJson = JsonConvert.SerializeObject(result);
                record.ReconciledAtUtc = DateTime.UtcNow;
                record.UpdatedAtUtc = DateTime.UtcNow;
                Persist();
            }
        }

        private Dictionary<string, ArmCommandRecord> Load()
        {
            if (!File.Exists(path)) return new Dictionary<string, ArmCommandRecord>(StringComparer.Ordinal);
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, ArmCommandRecord>>(json)
                ?? new Dictionary<string, ArmCommandRecord>(StringComparer.Ordinal);
        }

        private void Persist()
        {
            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(records, Formatting.Indented));
            if (File.Exists(path))
                File.Replace(temporaryPath, path, null);
            else
                File.Move(temporaryPath, path);
        }

        public void Dispose() { }
    }

    internal sealed class ArmCommandRecord
    {
        public string CommandId { get; set; }
        public string CorrelationId { get; set; }
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public string ResultJson { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime ReconciledAtUtc { get; set; }
    }
}
