using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using dotenv.net;
using Newtonsoft.Json;
using ArmController2;
using MethodRequest = Microsoft.Azure.Devices.Client.MethodRequest;
using MethodResponse = Microsoft.Azure.Devices.Client.MethodResponse;
using fairino;
using System.Text.Json;
using CouchDB.Client;

public class Program
{
    static CancellationTokenSource _cts = new CancellationTokenSource();
    static DeviceClient deviceClient;
    static ArmRobot robot;

    static string couchDbConn;
    static double timeOut = 30.0; // seconds
    
    static ArmController2.Publisher publisher = new ArmController2.Publisher();

    public static void Main(string[] args)
    {
        Console.WriteLine(nameof(UpdateStepStateMessages));
        //đăng kí sự kiện tắt kết nối cho serial
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Process exiting, disconnecting...");
            _cts.Cancel();
            //_monitorCts.Cancel();
        };
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        string connectionString = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR");

        //couchDbConn = Environment.GetEnvironmentVariable("COUCHDBURL");

        deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
        robot = new ArmRobot("192.168.58.2");

        // Register handlers
        //deviceClient.SetMethodHandlerAsync("enableRealTimePosition", GetRealTimePositionMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("disableRealTimePosition", StopRealTimePositionMethod, null).Wait();
        //deviceClient.SetMethodHandlerAsync("moveJ", MoveJMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("move", MoveMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("move2", Move2Method, robot).Wait();

        deviceClient.SetMethodHandlerAsync("runScript", RunScript, robot).Wait();

        Task.Delay(-1).Wait(); // Keep the application running
    }

    /// <summary>
    /// Run a script on the robot.
    /// {
    ///     "name": "script_name"
    /// }
    /// </summary>
    /// <param name="methodRequest"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    private static Task<MethodResponse> RunScript(MethodRequest methodRequest, object userContext)
    {

        try
        {
            var robot = (ArmRobot)userContext;
            var json = methodRequest.DataAsJson;

            var doc = JsonDocument.Parse(json);
            string name = doc.RootElement.GetProperty("name").GetString();

            string docId = doc.RootElement.GetProperty("docId").GetString();
            string stepId = doc.RootElement.GetProperty("stepId").GetString();
         
            int result = robot.RunScript(name);
            if(result != 0) throw new Exception("Run fail");
            
            //watching robot status
            _ = Task.Run(() => MonitorAsync(robot, docId, stepId, _cts.Token));

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("1"), 200));
        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Error"), 500));
        }
    }

    private static async Task MonitorAsync(ArmRobot robot, string docId, string stepId, CancellationToken cancellationToken)
    {
        var _monitorCts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(TimeSpan.FromSeconds(timeOut)).Token,
            cancellationToken
        );
        var token = _monitorCts.Token;
        //Create couchdb client
        var client = new CouchClient(couchDbConn);
        await Task.Delay(250);
        int stableCount = 0;
        int stepResult = 1; //1: Done 2: Failed
        bool isRunning = true;
        while (!token.IsCancellationRequested)
        {
            try
            {
                isRunning = robot.IsRunning();
                Console.WriteLine($"[{DateTime.Now}] Trang thai: {(isRunning ? "Dang chay" : "Da xong")}");

                if (!isRunning)
                {
                    stableCount++;
                    if (stableCount >= 1) 
                    {
                        var err = robot.robot.GetError();
                        if (err != 0)
                        {
                            stepResult = 2;
                        }
                        var message = new UpdateStepStateMessages(docId, stepId, stepResult);
                        publisher.PublishMessage(message: message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, nameof(UpdateStepStateMessages));
                        break;
                    }
                }
                else
                {
                    stableCount = 0;
                }

                await Task.Delay(250, token); 
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("MonitorAsync đã bị hủy do timeout.");
                //publish failed step message
                var message = new UpdateStepStateMessages(docId, stepId, 2);
                publisher.PublishMessage(message: message, exchangeName: QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY, nameof(UpdateStepStateMessages));
                break;
            }

            catch (Exception ex)
            {
                //Console.WriteLine($"Lỗi khi kiểm tra trạng thái: {ex.Message}");
                break;
            }
        }
    }


    private static Task<MethodResponse> MoveMethod(MethodRequest methodRequest, object userContext)
    {

        try
        {
            var robot = (ArmRobot)userContext;
            string data = methodRequest.DataAsJson;
            var coordinate = JsonConvert.DeserializeObject<Coordinate>(data);

            var jointPos = new JointPos(coordinate.J1, coordinate.J2, coordinate.J3, coordinate.J4, coordinate.J5, coordinate.J6);

            var result = robot.MoveFowardWithStableRotation(jointPos);
            if (result != 0) throw new Exception("Run fail");
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("1"), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }

    }

    private static Task<MethodResponse> Move2Method(MethodRequest methodRequest, object userContext)
    {

        try
        {
            var robot = (ArmRobot)userContext;
            string data = methodRequest.DataAsJson;
            var coordinate = JsonConvert.DeserializeObject<Coordinate>(data);

            var despose = new DescPose(coordinate.X, coordinate.Y, coordinate.Z, coordinate.RX, coordinate.RY, coordinate.RZ);


            double[] jPos = robot.GetCurrentJointPos();
            var currentJointPos = new JointPos(jPos[0], jPos[1], jPos[2], jPos[3], jPos[4], jPos[5]);
            var result = robot.MoveInverseWithStableRotation(despose, currentJointPos);
            if (result != 0) throw new Exception($"Run fail {result}");
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("1"), 200));

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }

    }

    private static Task<MethodResponse> MoveJMethod(MethodRequest methodRequest, object userContext)
    {

        try
        {
            var robot = (ArmRobot)userContext;
            string data = methodRequest.DataAsJson;
            var coordinate = JsonConvert.DeserializeObject<Coordinate>(data);

            var jointPos = new JointPos(coordinate.J1, coordinate.J2, coordinate.J3, coordinate.J4, coordinate.J5, coordinate.J6);

            var result = robot.MoveJ(jointPos);
            if (result != 0) throw new Exception("Run fail");
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("1"), 200));

        }
        catch (Exception e)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(e.Message), 500));
        }

    }

    private static Task<MethodResponse> GetRealTimePositionMethod(MethodRequest methodRequest, object userContext)
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        _cts = new CancellationTokenSource();

        ArmRobot armRobot = (ArmRobot)userContext;
        _ = Task.Run(() => SendPositionAsync(deviceClient, armRobot, _cts.Token));

        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Started"), 200));
    }

    private static Task<MethodResponse> StopRealTimePositionMethod(MethodRequest methodRequest, object userContext)
    {
        if (_cts != null)
        {
            Console.WriteLine("Stop WebSocket");
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Stopped"), 200));
        }

        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("Not running"), 200));
    }

    private static async Task SendPositionAsync(DeviceClient client, ArmRobot robot, CancellationToken cancellationToken)
    {
    
        
        string flag = "ws";
        string kioskId = Environment.GetEnvironmentVariable("KIOSKID") ?? "";
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("Send data");
                double[] jPos = robot.GetCurrentJointPos();
                double[] desPose = robot.GetForwardKinDesPose(jPos);

                var armCoordinateResponse = new ArmCoordinate
                {
                    InformationType = "info",
                    Coordinate = new Coordinate
                    {
                        J1 = (float)jPos[0],
                        J2 = (float)jPos[1],
                        J3 = (float)jPos[2],
                        J4 = (float)jPos[3],
                        J5 = (float)jPos[4],
                        J6 = (float)jPos[5],
                        X = (float)desPose[0],
                        Y = (float)desPose[1],
                        Z = (float)desPose[2],
                        RX = (float)desPose[3],
                        RY = (float)desPose[4],
                        RZ = (float)desPose[5],
                    },
                    TimeStamp = DateTime.UtcNow
                };

                string json = JsonConvert.SerializeObject(armCoordinateResponse, Formatting.Indented);
                var message = new Message(Encoding.UTF8.GetBytes(json))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };

                message.Properties.Add("kioskId", kioskId);
                message.Properties.Add("flag", flag);

                await client.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                var errorMsg = $"{DateTime.UtcNow}: Error sending message - {ex}\n";
                File.AppendAllText("error.log", errorMsg);
            }

            await Task.Delay(30000, cancellationToken);
        }
    }
}
