
using System.Text;
using Microsoft.Azure.Devices.Client;
using dotenv.net;
using IceMakerDevice.Libraries;
using System.Text.Json;
using static IceMakerDevice.Libraries.IceMakerStatusCommand;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using IceMakerMachine;
using Serilog;
using System.Threading;
using Azure;

public class Program
{
    static IceMachine iceMachine;
    static string DEVICE_CONNECTION_STRING;
    static string SERIAL_PORT;
    static int BAUD_RATE;


    static IRabbitMqPublisher<UpdateStepStateMessages> statePublisher;
    static IRabbitMqPublisher<UpdateStatusStepMsg> statusPublisher;
    static IRabbitMqPublisher<DeviceLabelMessage> deviceLabelMsgPublisher;


    static double timeOut = 180;
    static CancellationTokenSource _cts = new();

    public readonly static SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);

    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("========== ICE MACHINE CONTROLLER STARTING ==========");
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

            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            DEVICE_CONNECTION_STRING = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR")!;
            SERIAL_PORT = Environment.GetEnvironmentVariable("SERIAL_PORT") ?? "COM6";
            BAUD_RATE = int.TryParse(Environment.GetEnvironmentVariable("BAUD_RATE"), out int parsedBaud) ? parsedBaud : 115200;

            var deviceClient = DeviceClient.CreateFromConnectionString(DEVICE_CONNECTION_STRING);
            iceMachine = new IceMachine(SERIAL_PORT, BAUD_RATE);
            //đăng kí sự kiện tắt kết nối cho serial
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Console.WriteLine("Process exiting, disconnecting...");
                if (_cts != null) _cts.Cancel();
                iceMachine.Disconnect();

            };
            iceMachine.Connect();
            await deviceClient.SetMethodHandlerAsync("queryStatus", QueryStatusMethod, iceMachine);
            //await deviceClient.SetMethodHandlerAsync("queryParams", QueryParamsMethod, iceMachine);
            //await deviceClient.SetMethodHandlerAsync("setParams", SetParamsMethod, iceMachine);
            await deviceClient.SetMethodHandlerAsync("execute", ExcecuteMethod, iceMachine);
            await deviceClient.SetMethodHandlerAsync("powerOff", PowerOffMethod, iceMachine);

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
                               new StatusWatcher(iceMachine, GetDeviceIdFromConnStr(DEVICE_CONNECTION_STRING), statusPublisher, deviceLabelMsgPublisher));
                       })
                       .Build();

            _ = Task.Run(() => host.RunAsync());
            await Task.Delay(-1);
        }
        catch (Exception)
        {

            Console.WriteLine("Exception while closing ");
        }

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
    static Task<MethodResponse> QueryStatusMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var iceMachine = (IceMachine)userContext;
            var status = iceMachine.QueryStatus();
            var payload = System.Text.Json.JsonSerializer.Serialize(status);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }


    static Task<MethodResponse> QueryParamsMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var iceMachine = (IceMachine)userContext;
            var status = iceMachine.QueryParameters();
            var payload = System.Text.Json.JsonSerializer.Serialize(status);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    static Task<MethodResponse> SetParamsMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var iceMachine = (IceMachine)userContext;
            string data = methodRequest.DataAsJson;
            using var doc = JsonDocument.Parse(data);

            string language = doc.RootElement.GetProperty("language").GetString()!;
            double iceQty = doc.RootElement.GetProperty("iceQty").GetDouble();
            double waterQty = doc.RootElement.GetProperty("waterQty").GetDouble();
            double iceWaterQty = doc.RootElement.GetProperty("iceWaterQty").GetDouble();

            var status = iceMachine.SetParameters(language, iceQty, waterQty, iceWaterQty);
            var payload = System.Text.Json.JsonSerializer.Serialize(status);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    static Task<MethodResponse> ExcecuteMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var iceMachine = (IceMachine)userContext;
            string data = methodRequest.DataAsJson;
            using var doc = JsonDocument.Parse(data);

            byte type = doc.RootElement.GetProperty("type").GetByte();
            byte quantity = doc.RootElement.GetProperty("quantity").GetByte();
            string docId = doc.RootElement.GetProperty("docId").GetString();
            string stepId = doc.RootElement.GetProperty("stepId").GetString();

            IceMakerDispenseCommand status;

            semaphore.Wait();
            try
            {
                status = iceMachine.Excecute(type, quantity);
            }
            finally
            {
                semaphore.Release();
            }
            var payload = JsonSerializer.Serialize(status);

            //watching robot status

            _ = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine("Start monitoring machine status");
                    await MonitorAsync(iceMachine, docId, stepId, _cts.Token);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    static Task<MethodResponse> PowerOffMethod(MethodRequest methodRequest, object userContext)
    {
        try
        {
            var iceMachine = (IceMachine)userContext;
            var status = iceMachine.PowerOff();
            var payload = System.Text.Json.JsonSerializer.Serialize(status);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 200));
        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }
    }

    static int waitTime = 500;
    static async Task MonitorAsync(IceMachine machine, string docId, string stepId, CancellationToken cancellationToken)
    {
        using var _monitorCts = CancellationTokenSource.CreateLinkedTokenSource(
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
                isRunning = allStatus.Data2_WorkingStatus == IceMakerWorkingStatus.MakingBeverage;
                if (!isRunning)
                {
                    stableCount++;
                    if (stableCount >= 1) // đảm bảo đã xong hẳn (vd. kiểm tra 1 lần liên tục)
                    {

                        if (allStatus.Data2_WorkingStatus == IceMakerWorkingStatus.Unknown || allStatus.Data2_WorkingStatus == IceMakerWorkingStatus.FaultState)
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
            catch (Exception ex)
            {
                break;
            }
        }
    }
}










