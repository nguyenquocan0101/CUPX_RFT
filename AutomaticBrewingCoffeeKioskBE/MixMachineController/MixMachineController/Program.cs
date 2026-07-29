

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

            DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR")!;
            SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT")!;
            BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 9600;
            Console.WriteLine("[-] Environment variables loaded.");

            Console.WriteLine("[2] Connecting to Azure IoT Hub...");
            var deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
            Console.WriteLine("[-] Connected to Azure IoT Hub.");

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
                        new RabbitMqQueue(QueueConstants.QUEUE_STEP_UPDATE, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY)
                    }
                }
            };
            await provider.DeclareExchangeWithBindingAsync(exchangeBindings);
            publisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStepStateMessages>>();
            Console.WriteLine("[-] RabbitMQ configured and exchange/queue declared.");

            Console.WriteLine("[5] Registering IoT Hub method handlers...");
            //Define iothub invoke method handler
            await deviceClient.SetMethodHandlerAsync("run", RunOnTime, pinActivator);
            Console.WriteLine("[✓] IoT Hub method handlers registered.");

            Console.WriteLine("Mix Machine Controller is now running.");
            Console.WriteLine("========================================================");
            await Task.Delay(-1);
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
