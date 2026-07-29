using CouchDB.Driver;
using CouchDB.Driver.ChangesFeed;
using Newtonsoft.Json;
namespace WorkflowExecutorTest;
public class WorkflowHostService
{
    //danh sách thiết bị có trong kiosk
    private readonly List<DeviceForWorkFlow> _devices;
    //danh sách thiết bị dựa theo mẫu thiết bị 
    private readonly Dictionary<string, List<DeviceForWorkFlow>> _deviceGroup;


    private readonly CouchClient couchClient;
    private readonly StepSender sender;
    private readonly ICouchDatabase<WorkflowData> _workflowDb;

    public WorkflowHostService()
    {
        couchClient = new CouchClient("http://localhost:5984", builder => builder.UseBasicAuthentication("sa", "12345"));
        _workflowDb = couchClient.GetOrCreateDatabaseAsync<WorkflowData>("workflow").Result;

        //data mẫu cho thiết bị 
        var devicesInKiosk = new List<Device>
{
    new Device
    {
        DeviceId = "e518d6f8-376a-49aa-887b-2612cd17e7a9",
        DeviceModelId = "95cb2892-4f0e-4182-952b-102ed08aedb0", // Ice Maker model ID
        SerialNumber = "ICE-001",
        Name = "Ice Maker",
        Description = "Thiết bị làm đá tự động.",
         Status = "Active",
        X = 120.000m,
        Y = 80.000m,
        Z = 250.000m,
        RX = 0.000m,
        RY = 0.000m,
        RZ = 0.000m,
        J1 = 0.000m,
        J2 = 0.000m,
        J3 = 0.000m,
        J4 = 0.000m,
        J5 = 0.000m,
        J6 = 0.000m
    },
    new Device
    {
        DeviceId = "e518d6f8-376a-49aa-887b-2612cd17e7a9",
        DeviceModelId = "e8dc5804-279b-4de5-9125-44ce029eaa34", // Coffee machine model ID
        SerialNumber = "COF-001",
        Name = "CoffeeMachine",
        Description = "Máy pha cà phê tự động.",
         Status = "Active",
        X = 150.000m,
        Y = 90.000m,
        Z = 270.000m,
        RX = 10.000m,
        RY = -10.000m,
        RZ = 15.000m,
        J1 = 1.000m,
        J2 = 2.000m,
        J3 = 3.000m,
        J4 = 4.000m,
        J5 = 5.000m,
        J6 = 6.000m
    },
    new Device
    {
        DeviceId = "e518d6f8-376a-49aa-887b-2612cd17e7a9",
        DeviceModelId = "2f5bfe-dfaf-40d4-b930-b534da0ab8a9", // Arm model ID
        SerialNumber = "ARM-001",
        Name = "Arm",
        Description = "Cánh tay robot di chuyển vật.",
         Status = "Active",
        X = 197.595m,
        Y = 59.104m,
        Z = 281.879m,
        RX = 90.134m,
        RY = -79.859m,
        RZ = 94.872m,
        J1 = 95.008m,
        J2 = 0.011m,
        J3 = -158.000m,
        J4 = -122.150m,
        J5 = 0.024m,
        J6 = 0.008m
    },
    new Device
    {
        DeviceId = "e518d6f8-376a-49aa-887b-2612cd17e7a9",
        DeviceModelId = "8bc87f60-2a94-41f3-92da-cc60ec5ff38e", // Cup dropping device model ID
        SerialNumber = "CUP-001",
        Name = "Cup Dropping",
        Description = "Thiết bị thả ly vào vị trí pha chế.",
        Status = "Active",
        X = 130.000m,
        Y = 70.000m,
        Z = 240.000m,
        RX = 0.000m,
        RY = 0.000m,
        RZ = 0.000m,
        J1 = 0.000m,
        J2 = 0.000m,
        J3 = 0.000m,
        J4 = 0.000m,
        J5 = 0.000m,
        J6 = 0.000m
    }
};

        //Lấy thiết bị từ CouchDb
        _devices = devicesInKiosk
            .Where(d => d.Status == "Active")
            .Select(d => new DeviceForWorkFlow(d))
            .ToList();

        _deviceGroup = _devices
            .GroupBy(d => d.Device.DeviceModelId)
            .ToDictionary(g => g.Key, g => g.ToList());
        sender = new StepSender();


    }

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    Console.WriteLine("Listening for changes in CouchDB workflow database...");

    //    var tokenSource = new CancellationTokenSource();

    //    var options = new ChangesFeedOptions
    //    {
    //        LongPoll = true,
    //        IncludeDocs = true,
    //        Since = "_lastSeq"
    //    };

    //    await foreach (var change in _workflowDb.GetContinuousChangesAsync(options: options, filter: null, tokenSource.Token))
    //    {
    //        if (change.Deleted || change.Document == null)
    //            continue;            

    //        try
    //        {
    //            var workflow = change.Document;
    //            if (!workflow.Steps.Any(x => x.State.Equals("Pending")) || workflow.Steps.Any(x => x.State.Equals("Failed")))
    //            {
    //                tokenSource.Cancel();
    //            }
    //            Console.WriteLine("There is change");
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("Error while processing workflow step change. ");
    //        }
    //    }
    //}
}
