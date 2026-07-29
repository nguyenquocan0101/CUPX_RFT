using System.Text;
using System.Text.Json;
using dotenv.net;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;

namespace InhaleController
{
    public class Program
    {
        static PinActivator pinActivator;
        static string DEVICE_CONNECTION_STRING;
        static string SERIAL_PORT;
        static int BAUD_RATE;

        static IRabbitMqPublisher<UpdateStepStateMessages> publisher;

        static List<double> pumpTimeList = new();
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Console.WriteLine("========== INHALE MACHINE CONTROLLER STARTING ==========");
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
            pumpTimeList = configuration.GetSection("Pumps").Get<List<double>>();
            pinActivator = new PinActivator(SERIAL_PORT, BAUD_RATE, pumpTimeList);
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
                        new RabbitMqQueue(QueueConstants.QUEUE_DEVICE_UPDATE, QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY)
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
                        JournalPath = Path.Combine(Directory.GetCurrentDirectory(), ".local", "runtime", "controller-inhale.db")
                    });
                await localCommandHost.StartAsync();
            }
            else
            {
                Console.WriteLine("[5] Registering IoT Hub method handlers...");
                await deviceClient!.SetMethodHandlerAsync("runAll", RunAll, pinActivator);
                await deviceClient.SetMethodHandlerAsync("stopAll", StopAll, pinActivator);
                await deviceClient.SetMethodHandlerAsync("runOnTime", RunTarget, pinActivator);
                Console.WriteLine("[✓] IoT Hub method handlers registered.");
            }

            Console.WriteLine("Inhale Machine Controller is now running.");
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
                "runAll" => await RunAll(methodRequest, pinActivator),
                "stopAll" => await StopAll(methodRequest, pinActivator),
                "runOnTime" => await RunTarget(methodRequest, pinActivator),
                _ => throw new InvalidOperationException($"Unsupported inhale device method: {request.Method}")
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
        /// Handles the "runAll" method request from IoT Hub.
        /// parameter: {}
        /// </summary>
        /// <param name="methodRequest"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static Task<MethodResponse> RunAll(MethodRequest methodRequest, object userContext)
        {
            try
            {
                var pinActivator = (PinActivator)userContext;
                //var command = methodRequest.DataAsJson;
                string command = "#"; // Default command to run all pins    
                if (string.IsNullOrEmpty(command))
                {
                    throw new ArgumentException("Command cannot be null or empty");
                }
                var rs = pinActivator.RunAll(command).Result;

                var response = new MethodResponse(Encoding.UTF8.GetBytes("1"), 200);

                return Task.FromResult(response);
            }
            catch (Exception e)
            {
                var response = new MethodResponse(System.Text.Encoding.UTF8.GetBytes(e.Message), 500);
                return Task.FromResult(response);
            }
        }

        /// <summary>
        /// Handles the "stopAll" method request from IoT Hub.
        /// parameter: {}
        /// </summary>
        /// <param name="methodRequest"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static Task<MethodResponse> StopAll(MethodRequest methodRequest, object userContext)
        {
            try
            {
                var pinActivator = (PinActivator)userContext;
                //var command = methodRequest.DataAsJson;
                string command = "0"; // Default command to run all pins   
                if (string.IsNullOrEmpty(command))
                {
                    throw new ArgumentException("Command cannot be null or empty");
                }
                if (!pinActivator.StopAll(command).Result)
                {
                    throw new InvalidOperationException("Failed to activate pin");
                }
                var response = new MethodResponse(Encoding.UTF8.GetBytes("1"), 200);
                return Task.FromResult(response);
            }
            catch (Exception e)
            {
                var response = new MethodResponse(System.Text.Encoding.UTF8.GetBytes(e.Message), 500);
                return Task.FromResult(response);
            }
        }

        /// <summary>
        /// Handles the "runOnTime" method request from IoT Hub.
        /// parameter: {
        ///    "target1": 1,
        ///    "value1": 50,
        ///    "target2": 2,
        ///    "value2": 50,
        ///    "target3": 3,
        ///    "value3": 50,
        /// }
        /// </summary>
        /// <param name="methodRequest"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static Task<MethodResponse> RunTarget(MethodRequest methodRequest, object userContext)
        {
            try
            {
                string data = methodRequest.DataAsJson;
                using var doc = JsonDocument.Parse(data);
                string docId = doc.RootElement.GetProperty("docId").GetString();
                string stepId = doc.RootElement.GetProperty("stepId").GetString();

                var pinActivator = (PinActivator)userContext;

                //Buid command from json
                var parts = new List<string>();

                for (int i = 1; i <= pumpTimeList.Count; i++)
                {
                    if (doc.RootElement.TryGetProperty($"target{i}", out var targetProp) &&
                        doc.RootElement.TryGetProperty($"value{i}", out var valueProp))
                    {
                        string target = targetProp.ToString();
                        string value = valueProp.ToString();

                        if (!string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(value))
                        {
                            parts.Add($"{target}-{value}");
                        }
                    }
                }

                string command = string.Join("|", parts) + "|";


                if (string.IsNullOrEmpty(command))
                {
                    throw new ArgumentException("Command cannot be null or empty");
                }
                _ = Task.Run(async () =>
                {
                    var result = await pinActivator.RunTarget(command);
                    await PushStepStateMesssage(result, docId, stepId);
                });

                var response = new MethodResponse(Encoding.UTF8.GetBytes("1"), 200);
                return Task.FromResult(response);
            }
            catch (Exception e)
            {
                var response = new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500);
                return Task.FromResult(response);
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
