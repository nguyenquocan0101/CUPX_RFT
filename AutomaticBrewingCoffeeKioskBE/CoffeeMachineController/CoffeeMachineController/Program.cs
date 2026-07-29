using System.Text;
using System.Text.Json;
using CoffeeMachineController;
using dotenv.net;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Serilog;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;
using static CoffeeMachineController.SlaveStatusCommand;


public class Program
{
    static CoffeeMachine cf;
    static string DEVICE_CONNECTION_STRING;
    static string SERIAL_PORT;
    static int BAUD_RATE;

    static IRabbitMqPublisher<UpdateStepStateMessages> statePublisher;
    static IRabbitMqPublisher<UpdateStatusStepMsg> statusPublisher;
    static IRabbitMqPublisher<DeviceLabelMessage> deviceLabelMsgPublisher;

    static double timeOut = 240; //seconds

    public readonly static SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);

    static CancellationTokenSource _cts = new();
    public static async Task Main(string[] args)
    {
        Console.WriteLine("========== COFFEE MACHINE CONTROLLER STARTING ==========");
        Console.WriteLine("[0] Create File Logger");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: "Logs/log-.txt",         // Đặt tên file có hậu tố `-` để Serilog tự thêm ngày
                rollingInterval: RollingInterval.Day, // Tạo file mới mỗi ngày
                retainedFileCountLimit: 7,     // Giữ lại 7 file gần nhất, có thể bỏ nếu không giới hạn
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}"
            )
            .CreateLogger();
        Console.WriteLine("[-] File Logger created.");

        // Load environment variables
        Console.WriteLine("[1] Loading environment variables...");
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

        DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR")!;
        SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT")!;
        BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 115200;
        Console.WriteLine("[-] Environment variables loaded.");

        // Initialize Azure IoT device client
        Console.WriteLine("[2] Connecting to Azure IoT Hub...");
        var deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
        Console.WriteLine("[-] Azure IoT Hub connected.");

        // Initialize Coffee Machine
        Console.WriteLine("[-] Initializing Coffee Machine on port {0} with baud rate {1}...", SERIAL_PORT, BAUD_RATE);
        cf = new CoffeeMachine(SERIAL_PORT, BAUD_RATE);
        //cf.Connect();
        // Handle process exit
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("[!] Process exiting, disconnecting...");
            if (_cts != null) _cts.Cancel();
            cf.Disconnect();
        };

        Console.WriteLine("[-] Coffee Machine connected.");

        // Set IoT Hub method handlers
        Console.WriteLine("[4] Registering IoT direct method handlers...");
        await deviceClient.SetMethodHandlerAsync("getStatus", GetStatusMethod, cf);
        await deviceClient.SetMethodHandlerAsync("makeDrink", MakeDrinkMethod, cf);
        await deviceClient.SetMethodHandlerAsync("shutdown", ShutDownMethod, cf);
        await deviceClient.SetMethodHandlerAsync("clean", CleanMethod, cf);

        //demo update 2 step cùng 1 lúc
        //await deviceClient.SetMethodHandlerAsync("cc", CC, null);
        Console.WriteLine("[-] Method handlers registered.");

        // Start background services
        Console.WriteLine("[5] Starting background services...");
        Console.WriteLine("========================================================");
        Console.WriteLine("Coffee Machine Controller is now running.");
        Log.Information("Coffee Machine Controller started");
        Console.WriteLine("========================================================");

        var host = Host.CreateDefaultBuilder(args)
             .ConfigureServices((context, services) =>
             {
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
                            new RabbitMqQueue(QueueConstants.QUEUE_DEVICE_UPDATE,QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY)
                        }
                    }
                };
                 provider.DeclareExchangeWithBindingAsync(exchangeBindings);
                 statePublisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStepStateMessages>>();
                 statusPublisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStatusStepMsg>>();
                 deviceLabelMsgPublisher = provider.GetRequiredService<IRabbitMqPublisher<DeviceLabelMessage>>();
                 services.AddHostedService(src =>
                    new StatusWatcher(cf, GetDeviceIdFromConnStr(DEVICE_CONNECTION_STRING), statusPublisher, deviceLabelMsgPublisher));
             })
            .Build();

        _ = Task.Run(() => host.RunAsync());

        await Task.Delay(-1);
    }

    private static string GetDeviceIdFromConnStr(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) return string.Empty;
        string deviceId = connectionString
            .Split(';')
            .FirstOrDefault(part => part.StartsWith("DeviceId=", StringComparison.OrdinalIgnoreCase))?
            .Split('=')[1];

        return deviceId ?? string.Empty;
    }

    /*
    Method name: getStatus
    Parameter Json: {}
    */
    static Task<MethodResponse> GetStatusMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            CoffeeMachine cf = (CoffeeMachine)userContext;
            var slaveStatus = cf.QueryStatus();
            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(slaveStatus);
            LogInvokeMethod(methodRequest.Name, true);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (Exception e)
        {
            LogInvokeMethod(methodRequest.Name, false);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    /*
    Method name: makeDrink
    Parameter Json: 
    {
        drinkId: "1"
    }
     */
    static Task<MethodResponse> MakeDrinkMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            DrinkOrCleanCommand response;
            CoffeeMachine cf = (CoffeeMachine)userContext;
            string data = methodRequest.DataAsJson;
            using var doc = JsonDocument.Parse(data);
            int drinkId = doc.RootElement.GetProperty("drinkId").GetInt32();
            string docId = doc.RootElement.GetProperty("docId").GetString();
            string stepId = doc.RootElement.GetProperty("stepId").GetString();

            semaphore.Wait();
            try
            {
                response = cf.MakeDrink(drinkId);
            }
            finally
            {
                semaphore.Release();
            }

            //watching robot status
            _ = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine("Start monitoring machine status");
                    await MonitorAsync(cf, docId, stepId, _cts.Token);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            


            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(response);
            LogInvokeMethod(methodRequest.Name, true);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (ArgumentException e)
        {
            Log.Warning("Invoke method: {Method}. Exception: {ex}", methodRequest.Name, "Drink Id is invalid");
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Drink Id is invalid"), 400));
        }
        catch (Exception e)
        {
            Log.Warning("Invoke method: {Method}. Exception: {ex}", methodRequest.Name, e.Message);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    /*
    Method name: shutdown
    Parameter Json: {}
     */
    static Task<MethodResponse> ShutDownMethod(MethodRequest methodRequest, object userContext)
    {
        ShutdownCommand shutdownResponse;
        try
        {
            CoffeeMachine cf = (CoffeeMachine)userContext;
            semaphore.Wait();
            try
            {
                shutdownResponse = cf.Shutdown();
            }
            finally
            {
                semaphore.Release();
            }

            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(shutdownResponse);
            LogInvokeMethod(methodRequest.Name, true);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (Exception e)
        {
            Log.Warning("Invoke method: {Method}. Exception: {ex}", methodRequest.Name, e.Message);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    /*
    Method name: clean
    Parameter Json: 
    {
        actionId: "1"
    }
     */
    static Task<MethodResponse> CleanMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            CoffeeMachine cf = (CoffeeMachine)userContext;

            string data = methodRequest.DataAsJson;

            using var doc = JsonDocument.Parse(data);
            string? actionId = doc.RootElement.GetProperty("actionId").GetString();
            string docId = doc.RootElement.GetProperty("docId").GetString();
            string stepId = doc.RootElement.GetProperty("stepId").GetString();
            if (string.IsNullOrEmpty(actionId)) throw new ArgumentNullException(nameof(actionId));


            DrinkOrCleanCommand response;
            semaphore.Wait();
            try
            {
                response = cf.Clean(Enum.Parse<DrinkOrCleanCommand.CommandAction>(actionId));
            }
            finally
            {
                semaphore.Release();
            }

            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(response);

            //watching robot status
            _ = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine("Start monitoring machine status");
                    await MonitorAsync(cf, docId, stepId, _cts.Token);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            LogInvokeMethod(methodRequest.Name, true);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (ArgumentNullException)
        {
            Log.Warning("Invoke method: {Method}. Exception: {ex}", methodRequest.Name, "Invalid Command Action");
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Invalid Command Action"), 400));
        }
        catch (Exception e)
        {
            Log.Warning("Invoke method: {Method}. Exception: {ex}", methodRequest.Name, e.Message);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    private static void LogInvokeMethod(string methodName, bool isSuccess)
    {
        if (isSuccess)
            Log.Information("Invoke method: {Method}. Result: Success", methodName);
        else
            Log.Warning("Invoke method: {Method}. Result: Fail", methodName);
    }

    static int waitTime = 500;
    static async Task MonitorAsync(CoffeeMachine machine, string docId, string stepId, CancellationToken cancellationToken)
    {
        var _monitorCts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(TimeSpan.FromSeconds(timeOut)).Token,
            cancellationToken
        );

        var token = _monitorCts.Token;
        await Task.Delay(waitTime);
        int stableCount = 0;
        int stepResult = 1; //1: Done, 2: Failed
        bool isRunning = true;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var allStatus = machine.QueryStatus();
                isRunning = allStatus.Data1.CurrentSystemStatus == SystemStatus.Running;
                if (!isRunning)
                {
                    stableCount++;
                    if (stableCount >= 1) // đảm bảo đã xong hẳn (vd. kiểm tra 1 lần liên tục)
                    {
                        if (allStatus.Data1.CurrentSystemStatus == SystemStatus.Unknown)
                        {
                            stepResult = 2;
                        }

                        //* push message to step-update queue
                        Console.WriteLine("Push step state to queue");
                        var message = new UpdateStepStateMessages(docId, stepId, stepResult);
                        var props = new BasicProperties
                        {
                            ContentType = "application/json",
                            DeliveryMode = DeliveryModes.Persistent,
                            Type = nameof(UpdateStepStateMessages)
                        };
                        await statePublisher.PublishMessageAsync(message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, props);
                        Log.Information("Machine run completed. Result: {rs}", stepResult == 1 ? "Success" : "Failed");

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Dang chạy");
                    stableCount = 0;
                }

                await Task.Delay(waitTime, token);
            }
            catch (OperationCanceledException )
            {
                Console.WriteLine("MonitorAsync is cancled because of timeout.");
                var message = new UpdateStepStateMessages(docId, stepId, 2);
                var props = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    Type = nameof(UpdateStepStateMessages)
                };
                await statePublisher.PublishMessageAsync(message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, props);
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {e}", e.Message);
                break;
            }
        }
    }
}



