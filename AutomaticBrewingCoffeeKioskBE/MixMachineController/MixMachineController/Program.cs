

using System.Text;
using System.Text.Json;
using dotenv.net;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;

namespace MixMachineController
{
    public class Program
    {
        static PinActivator pinActivator;
        static string DEVICE_CONNECTION_STRING;
        static string SERIAL_PORT;
        static int BAUD_RATE;

        static IRabbitMqPublisher<UpdateStepStateMessages> publisher;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("========== MIX MACHINE CONTROLLER STARTING ==========");
            Console.WriteLine("[1] Loading environment variables...");
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

            var localHardware = string.Equals(Environment.GetEnvironmentVariable("HARDWARE_MODE"), "real", StringComparison.OrdinalIgnoreCase);
            DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR") ?? string.Empty;
            SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT") ?? throw new InvalidOperationException("SERIAL_PORT is required.");
            BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 9600;
            Console.WriteLine("[-] Environment variables loaded.");

            DeviceClient? deviceClient = null;
            var deviceId = localHardware
                ? Environment.GetEnvironmentVariable("DEVICE_ID") ?? throw new InvalidOperationException("DEVICE_ID is required in HARDWARE_MODE=real.")
                : GetDeviceIdFromConnStr(DEVICE_CONNECTION_STRING);
            if (!localHardware)
            {
                Console.WriteLine("[2] Connecting to Azure IoT Hub...");
                deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
                Console.WriteLine("[-] Connected to Azure IoT Hub.");
            }

            Console.WriteLine("[3] Connecting to device via Serial Port...");
            pinActivator = new PinActivator(SERIAL_PORT, BAUD_RATE);
            pinActivator.Connect();
            Console.WriteLine($"[-] Connected to device on {SERIAL_PORT} @ {BAUD_RATE} baud.");

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Console.WriteLine("[!] Process exiting, disconnecting...");
                pinActivator.DisConnect();
            };

            Console.WriteLine("[4] Setting up RabbitMQ connection...");
            var services = new ServiceCollection();
            services.AddOriginRabitMq(
                Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest");
            var provider = services.BuildServiceProvider();
            var exchangeBindings = new List<ExchangeBindingConfig>
            {
                new ExchangeBindingConfig
                {
                    ExchangeName = QueueConstants.EXCHANGE_NAME,
                    ExchangeType = ExchangeType.Direct,
                    Queues = new List<RabbitMqQueue>
                    {
                        new RabbitMqQueue(QueueConstants.QUEUE_STEP_UPDATE, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY)
                    }
                }
            };
            await provider.DeclareExchangeWithBindingAsync(exchangeBindings);
            publisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStepStateMessages>>();
            Console.WriteLine("[-] RabbitMQ configured and exchange/queue declared.");

            if (localHardware)
            {
                var localCommandHost = new LocalDeviceCommandHost(
                    deviceId,
                    HandleLocalCommandAsync,
                    new LocalDeviceCommandHostOptions
                    {
                        JournalPath = Path.Combine(Directory.GetCurrentDirectory(), ".local", "runtime", "controller-mix.db")
                    });
                await localCommandHost.StartAsync();
            }
            else
            {
                Console.WriteLine("[5] Registering IoT Hub method handlers...");
                await deviceClient!.SetMethodHandlerAsync("run", RunOnTime, pinActivator);
                Console.WriteLine("[✓] IoT Hub method handlers registered.");
            }

            Console.WriteLine("Mix Machine Controller is now running.");
            Console.WriteLine("========================================================");
            await Task.Delay(-1);
        }

        private static string GetDeviceIdFromConnStr(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return string.Empty;
            return connectionString
                .Split(';')
                .FirstOrDefault(part => part.StartsWith("DeviceId=", StringComparison.OrdinalIgnoreCase))?
                .Split('=', 2).ElementAtOrDefault(1) ?? string.Empty;
        }

        private static async Task<DeviceCommandResult> HandleLocalCommandAsync(DeviceCommandRequest request, CancellationToken cancellationToken)
        {
            var methodRequest = new MethodRequest(
                request.Method,
                Encoding.UTF8.GetBytes(LocalDeviceCommandPayload.ToJson(request.Parameters)));
            var response = request.Method switch
            {
                "run" => await RunOnTime(methodRequest, pinActivator),
                _ => throw new InvalidOperationException($"Unsupported mix device method: {request.Method}")
            };
            return new DeviceCommandResult(
                request.CommandId,
                request.SchemaVersion,
                request.CorrelationId,
                request.DeviceId,
                response.Status == 200 ? "Completed" : "Failed",
                new Dictionary<string, string> { ["result"] = response.ResultAsJson },
                response.Status == 200 ? null : "DEVICE_METHOD_FAILURE",
                response.Status == 200 ? null : response.ResultAsJson,
                DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Handles the "runOnTime" method request from IoT Hub.
        /// parameter: {
        ///     "value": "10.5",
        /// }
        /// </summary>
        /// <param name="methodRequest"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private async static Task<MethodResponse> RunOnTime(MethodRequest methodRequest, object userContext)
        {
            try
            {
                string data = methodRequest.DataAsJson;
                using var doc = JsonDocument.Parse(data);
                string command = doc.RootElement.GetProperty("value").GetString();
                string docId = doc.RootElement.GetProperty("docId").GetString();
                string stepId = doc.RootElement.GetProperty("stepId").GetString();

                var pinActivator = (PinActivator)userContext;
                if (string.IsNullOrEmpty(command))
                {
                    throw new ArgumentException("Command cannot be null or empty");
                }
               
                _ = Task.Run(async () =>
                {
                    var result = await pinActivator.RunOnTime(command);
                    await PushStepStateMesssage(result, docId, stepId);
                });

                var response = new MethodResponse(Encoding.UTF8.GetBytes(""), 200);

                return await Task.FromResult(response);
            }
            catch (Exception e)
            {
                var response = new MethodResponse(System.Text.Encoding.UTF8.GetBytes(e.Message), 500);
                return await Task.FromResult(response);
            }
        }

        static async Task PushStepStateMesssage(bool success, string docId, string stepId)
        {
            int stepResult = success ? 1 : 2; //1: Done , 2: Failed

            Console.WriteLine("Push step state to queue");
            var message = new UpdateStepStateMessages(docId, stepId, stepResult);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Type = nameof(UpdateStepStateMessages)
            };
            await publisher.PublishMessageAsync(message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, props);
        }
    }
}
