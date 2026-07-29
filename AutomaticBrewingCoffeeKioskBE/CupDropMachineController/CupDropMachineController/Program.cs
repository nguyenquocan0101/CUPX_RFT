using System.Text.Json;
using System.Text;
using CupDropMachineController;
using Microsoft.Azure.Devices.Client;
using dotenv.net;
using Shared.RabbitMqPublisher;
using Shared.MessageStore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;
using System.Threading;
using Azure;

public class Program
{
    static CupDroppingMachine cd;
    static string DEVICE_CONNECTION_STRING;
    static string SERIAL_PORT;
    static int BAUD_RATE;

    static IRabbitMqPublisher<UpdateStepStateMessages> statePublisher;
    static IRabbitMqPublisher<UpdateStatusStepMsg> statusPublisher;
    static IRabbitMqPublisher<DeviceLabelMessage> deviceLabelPublisher;

    static double timeOut = 30;
    static CancellationTokenSource _cts = new();

    public readonly static SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);

    public static async Task Main(string[] args)
    {

        Console.WriteLine("========== CUP DROPPING MACHINE CONTROLLER STARTING ==========");
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
        Console.WriteLine("[✓] File Logger created.");
        Console.WriteLine("[1] Load Environment");
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        var localHardware = string.Equals(Environment.GetEnvironmentVariable("HARDWARE_MODE"), "real", StringComparison.OrdinalIgnoreCase);
        DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR") ?? string.Empty;
        SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT") ?? throw new InvalidOperationException("SERIAL_PORT is required.");
        BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 115200;
        Console.WriteLine("[✓] Environment Loaded");

        Console.WriteLine("[2] Initialize variables");
        DeviceClient? deviceClient = null;
        var deviceId = localHardware
            ? Environment.GetEnvironmentVariable("DEVICE_ID") ?? throw new InvalidOperationException("DEVICE_ID is required in HARDWARE_MODE=real.")
            : GetDeviceIdFromConnStr(DEVICE_CONNECTION_STRING);
        if (!localHardware)
            deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
        Console.WriteLine("[2] Initializing Cup Dropping Machine on port {0} with baud rate {1}...", SERIAL_PORT, BAUD_RATE);
        cd = new CupDroppingMachine(SERIAL_PORT, BAUD_RATE);
        //đăng kí sự kiện tắt kết nối cho serial
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("[!] Process exiting, disconnecting...");
            _cts.Cancel();
            cd.Disconnect();
        };
        cd.Connect();
        Console.WriteLine("[✓] Initialize variables completed");

        if (localHardware)
        {
            var localCommandHost = new LocalDeviceCommandHost(
                deviceId,
                HandleLocalCommandAsync,
                new LocalDeviceCommandHostOptions
                {
                    JournalPath = Path.Combine(Directory.GetCurrentDirectory(), ".local", "runtime", "controller-cup-drop.db")
                });
            await localCommandHost.StartAsync(_cts.Token);
        }
        else
        {
            Console.WriteLine("[3] Declare invoke methods");
            await deviceClient!.SetMethodHandlerAsync("getStatus", GetStatusMethod, cd);
            await deviceClient.SetMethodHandlerAsync("dropCup", DropCupMethod, cd);
            await deviceClient.SetMethodHandlerAsync("shutdown", ShutDownMethod, cd);
        }
        Console.WriteLine("[✓] Declare invoke methods completed");

        Console.WriteLine("[4] Starting background services...");
        Console.WriteLine("========================================================");
        Console.WriteLine("Cup Dropping Machine Controller is now running.");
        Log.Information("Cup Dropping Machine Controller started");
        Console.WriteLine("========================================================");
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
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
                            new RabbitMqQueue(QueueConstants.QUEUE_DEVICE_UPDATE,QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY)
                        }
                    }
                };
                provider.DeclareExchangeWithBindingAsync(exchangeBindings);
                statePublisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStepStateMessages>>();
                statusPublisher = provider.GetRequiredService<IRabbitMqPublisher<UpdateStatusStepMsg>>();
                deviceLabelPublisher = provider.GetRequiredService<IRabbitMqPublisher<DeviceLabelMessage>>();

                services.AddHostedService(src =>
                    new StatusWatcher(cd, deviceId, statusPublisher, deviceLabelPublisher));
            })
            .Build();
        _ = Task.Run(() => host.RunAsync());

        await Task.Delay(-1);

    }

    private static async Task<DeviceCommandResult> HandleLocalCommandAsync(DeviceCommandRequest request, CancellationToken cancellationToken)
    {
        var methodRequest = new MethodRequest(
            request.Method,
            Encoding.UTF8.GetBytes(LocalDeviceCommandPayload.ToJson(request.Parameters)));
        var response = request.Method switch
        {
            "getStatus" => await GetStatusMethod(methodRequest, cd),
            "dropCup" => await DropCupMethod(methodRequest, cd),
            "shutdown" => await ShutDownMethod(methodRequest, cd),
            _ => throw new InvalidOperationException($"Unsupported cup-drop device method: {request.Method}")
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
            CupDroppingMachine cf = (CupDroppingMachine)userContext;
            var slaveStatus = cf.QueryStatus();
            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(slaveStatus);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    /*
    Method name: dropCup
    Parameter Json: {}
     */
    static Task<MethodResponse> DropCupMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            CupDroppingMachine cd = (CupDroppingMachine)userContext;
            string data = methodRequest.DataAsJson;
            using var doc = JsonDocument.Parse(data);
            string docId = doc.RootElement.GetProperty("docId").GetString();
            string stepId = doc.RootElement.GetProperty("stepId").GetString();

            DispenseBeverageCommand response;
            semaphore.Wait();
            try
            {
                response = cd.DropOneCup();
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
                    await MonitorAsync(cd, docId, stepId, _cts.Token);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            string result = System.Text.Json.JsonSerializer.Serialize(response);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            //return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(string.Empty), 200));
        }

        catch (ArgumentException)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Mã nước không hợp lệ"), 400));
        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    /*
    Method name: shutdown
    Parameter Json: {}
     */
    static Task<MethodResponse> ShutDownMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            CupDroppingMachine cf = (CupDroppingMachine)userContext;
            var shtuDownCommand = cf.Shutdown();
            // Trả về phản hồi cho cloud
            string result = System.Text.Json.JsonSerializer.Serialize(shtuDownCommand);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    static int waitTime = 500;
    static async Task MonitorAsync(CupDroppingMachine machine, string docId, string stepId, CancellationToken cancellationToken)
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
                isRunning = allStatus.Data2.CurrentSystemStatus == SystemStatus.CupDroppingInProgress;
                Console.WriteLine(nameof(allStatus.Data2.CurrentSystemStatus));
                if (!isRunning)
                {
                    stableCount++;
                    if (stableCount >= 1) // đảm bảo đã xong hẳn (vd. kiểm tra 1 lần liên tục)
                    {
                        if (allStatus.Data2.CurrentSystemStatus == SystemStatus.Unknown || allStatus.Data2.CurrentSystemStatus == SystemStatus.HasFault)
                        {
                            stepResult = 2;
                        }
                        var message = new UpdateStepStateMessages(docId, stepId, stepResult);
                        var props = new BasicProperties
                        {
                            ContentType = "application/json",
                            DeliveryMode = DeliveryModes.Persistent,
                            Type = nameof(UpdateStepStateMessages)
                        };
                        await statePublisher.PublishMessageAsync(message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, props);
                        Log.Information("Machine run completed. Result: {rs}", stepResult == 0 ? "Success" : "Failed");
                        break;
                    }
                }
                else
                {
                    stableCount = 0;
                }

                await Task.Delay(waitTime, token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("MonitorAsync đã bị hủy do timeout.");
                var message = new UpdateStepStateMessages(docId, stepId, stepResult);
                var props = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    Type = nameof(UpdateStepStateMessages)
                };
                await statePublisher.PublishMessageAsync(message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, props);
                break;
            }
            catch (Exception ex)
            {
                break;
            }
        }
    }

}








