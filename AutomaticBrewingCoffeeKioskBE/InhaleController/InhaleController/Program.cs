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

            DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR")!;
            SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT")!;
            BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 9600;
            Console.WriteLine("[-] Environment variables loaded.");

            Console.WriteLine("[2] Connecting to Azure IoT Hub...");
            var deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
            Console.WriteLine("[-] Connected to Azure IoT Hub.");

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
            services.AddOriginRabitMq("localhost", "guest", "guest");
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

            Console.WriteLine("[5] Registering IoT Hub method handlers...");
            //Define iothub invoke method handler
            await deviceClient.SetMethodHandlerAsync("runAll", RunAll, pinActivator);
            await deviceClient.SetMethodHandlerAsync("stopAll", StopAll, pinActivator);
            await deviceClient.SetMethodHandlerAsync("runOnTime", RunTarget, pinActivator);
            Console.WriteLine("[✓] IoT Hub method handlers registered.");

            Console.WriteLine("Inhale Machine Controller is now running.");
            Console.WriteLine("========================================================");
            await Task.Delay(-1);
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
