
using Newtonsoft.Json;
using WorkflowExecutorTest;
using WorkflowExecutorTest.CouchDbInteraction;


string json = await File.ReadAllTextAsync("D:\\Workspace\\_GITHUB\\AutomaticBrewingCoffee_Kiosk_BE\\ABC_Kiosk_BE\\Tests\\WorkflowExecutorTest\\blackcoffee.json");
var workflow = JsonConvert.DeserializeObject<Workflow>(json);
//StepSender executor = new StepSender(workflow, devices);

Console.WriteLine("khoi tao executor hoan thanh");
//await executor.ExecuteAsync(workflow.WorkflowId);

//Host.CreateDefaultBuilder(args)
//    .ConfigureServices((context, services) =>
//    {
//        services.AddHostedService<WorkflowHostService>();
        
//    })
//    .Build()
//    .Run();
Console.WriteLine();