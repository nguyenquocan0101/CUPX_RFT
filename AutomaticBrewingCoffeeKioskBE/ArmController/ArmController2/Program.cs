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
using Newtonsoft.Json.Linq;

public class Program
{
    static CancellationTokenSource _cts = new CancellationTokenSource();
    static DeviceClient deviceClient;
    static ArmRobot robot;
    static LocalArmCommandHost localCommandHost;

    static string couchDbConn;
    static double timeOut = 30.0; // seconds
    
    static ArmController2.Publisher publisher;

    public static void Main(string[] args)
    {
        string reconcileCommand;
        if (TryGetArgument(args, "--reconcile", out reconcileCommand))
        {
            var journalPath = GetArgument(args, "--journal") ?? Path.Combine(Environment.CurrentDirectory, ".local", "runtime", "controller-arm.json");
            var resolution = GetArgument(args, "--resolution") ?? "Failed";
            var journal = new ArmCommandJournal(journalPath);
            journal.Initialize();
            journal.Reconcile(reconcileCommand, resolution);
            Console.WriteLine("Arm command reconciled: " + reconcileCommand + " resolution=" + resolution);
            return;
        }

        Console.WriteLine(nameof(UpdateStepStateMessages));
        //đăng kí sự kiện tắt kết nối cho serial
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Process exiting, disconnecting...");
            _cts.Cancel();
            if (localCommandHost != null) localCommandHost.Dispose();
            //_monitorCts.Cancel();
        };
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        bool localHardware = string.Equals(
            Environment.GetEnvironmentVariable("HARDWARE_MODE"), "real", StringComparison.OrdinalIgnoreCase);
        string connectionString = Environment.GetEnvironmentVariable("DEVICE_PRIMARY_CONN_STR");
        string deviceId = Environment.GetEnvironmentVariable("DEVICE_ID") ?? "arm-controller";
        couchDbConn = Environment.GetEnvironmentVariable("COUCHDBURL") ?? "http://localhost:5984";

        if (!localHardware)
            deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
        publisher = new ArmController2.Publisher();
        robot = new ArmRobot(Environment.GetEnvironmentVariable("ARM_ROBOT_IP") ?? "192.168.58.2");

        // Register handlers
        //deviceClient.SetMethodHandlerAsync("enableRealTimePosition", GetRealTimePositionMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("disableRealTimePosition", StopRealTimePositionMethod, null).Wait();
        //deviceClient.SetMethodHandlerAsync("moveJ", MoveJMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("move", MoveMethod, robot).Wait();
        //deviceClient.SetMethodHandlerAsync("move2", Move2Method, robot).Wait();

        if (localHardware)
        {
            localCommandHost = new LocalArmCommandHost(
                deviceId,
                HandleLocalCommand,
                Environment.GetEnvironmentVariable("LOCAL_COMMAND_JOURNAL")
                    ?? Path.Combine(Environment.CurrentDirectory, ".local", "runtime", "controller-arm.json"));
            localCommandHost.Start();
        }
        else
        {
            deviceClient.SetMethodHandlerAsync("runScript", RunScript, robot).Wait();
        }

        Task.Delay(-1).Wait(); // Keep the application running
    }

    private static ArmDeviceCommandResult HandleLocalCommand(ArmDeviceCommandRequest request)
    {
        if (!string.Equals(request.Method, "runScript", StringComparison.Ordinal))
            throw new InvalidOperationException("Unsupported Arm method: " + request.Method);

        var methodRequest = new MethodRequest(
            request.Method,
            Encoding.UTF8.GetBytes(ToJson(request.Parameters)));
        var response = RunScript(methodRequest, robot).GetAwaiter().GetResult();
        return new ArmDeviceCommandResult
        {
            CommandId = request.CommandId,
            SchemaVersion = request.SchemaVersion,
            CorrelationId = request.CorrelationId,
            DeviceId = request.DeviceId,
            Status = response.Status == 200 ? "Completed" : "Failed",
            Payload = new System.Collections.Generic.Dictionary<string, string> { ["result"] = response.ResultAsJson },
            ErrorCode = response.Status == 200 ? null : "DEVICE_METHOD_FAILURE",
            ErrorMessage = response.Status == 200 ? null : response.ResultAsJson,
            CompletedAtUtc = DateTime.UtcNow
        };
    }

    private static string ToJson(System.Collections.Generic.Dictionary<string, string> parameters)
    {
        if (parameters != null && parameters.ContainsKey("raw") && !string.IsNullOrWhiteSpace(parameters["raw"]))
            return parameters["raw"];

        var values = new System.Collections.Generic.Dictionary<string, object>();
        if (parameters != null)
        {
            foreach (var pair in parameters)
            {
                try { values[pair.Key] = JToken.Parse(pair.Value); }
                catch (Newtonsoft.Json.JsonException) { values[pair.Key] = pair.Value; }
            }
        }
        return JsonConvert.SerializeObject(values);
    }

    private static bool TryGetArgument(string[] args, string name, out string value)
    {
        value = GetArgument(args, name);
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string GetArgument(string[] args, string name)
    {
        var prefix = name + "=";
        foreach (var argument in args ?? new string[0])
            if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return argument.Substring(prefix.Length);
        return null;
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
