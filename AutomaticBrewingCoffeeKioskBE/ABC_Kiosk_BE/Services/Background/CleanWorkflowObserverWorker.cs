using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver;
using Domain.CouchDbModels;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Services.Utils;
using CouchDB.Driver.DatabaseApiMethodOptions;
using CouchDB.Driver.Exceptions;
using System.Text.Json.Nodes;
using CouchDb.Domain.Enums;
using RabbitMQ.Client;
using Shared.MessageStore;
using static Domain.MessageRecords;
using System.Text;
using Services.Interfaces;


public class CleanWorkflowObserverWorker : BackgroundService
{
    private readonly CouchClient _couchClient;
    private readonly ICouchDatabase<WorkflowData> _workflowData;
    private readonly ICouchDatabase<DeviceDocument> _deviceData;
    private readonly ILogger<WorkflowObserverWorker> _logger;
    private readonly string _lastSeqTxtpath;
    private string _lastSeq;
    private readonly ServiceClient _serviceClient;
    private readonly IServiceProvider _serviceProvider;

    private readonly IModel _workflowChannel;
    private readonly IModel _stepAndWfChannel;

    public CleanWorkflowObserverWorker(IServiceProvider provider, IConfiguration configuration,
        ILogger<WorkflowObserverWorker> logger, [FromKeyedServices(QueueConstants.QUEUE_WORKFLOW_EXECUTE)] IModel workflowChannel,
        [FromKeyedServices(QueueConstants.QUEUE_STEP_UPDATE)] IModel stepAndWfChannel)
    {
        _logger = logger;
        var url = configuration["CouchDB:Url"]!;
        var username = configuration["CouchDB:Username"]!;
        var pwd = configuration["CouchDB:Pwd"]!;
        _couchClient = new CouchClient(url,
            builder => builder.UseBasicAuthentication(username, pwd));

        _workflowData = _couchClient.GetOrCreateDatabaseAsync<WorkflowData>(StringHelper.GetCouchDbDatabaseNameFormat(nameof(WorkflowData))).Result;
        _deviceData = _couchClient.GetOrCreateDatabaseAsync<DeviceDocument>(StringHelper.GetCouchDbDatabaseNameFormat(nameof(DeviceDocument))).Result;

        _lastSeqTxtpath = Path.Combine(Directory.GetCurrentDirectory(), "clean_workflow_seq.txt");
        _lastSeq = File.Exists(_lastSeqTxtpath) ? File.ReadAllText(_lastSeqTxtpath) : "0";
        if (!File.Exists(_lastSeqTxtpath))
            File.WriteAllText(_lastSeqTxtpath, _lastSeq);

        _serviceClient = ServiceClient.CreateFromConnectionString(configuration["AzureServiceConn"]!);
        _serviceProvider = provider;

        _workflowChannel = workflowChannel;
        _stepAndWfChannel = stepAndWfChannel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tokenSource = new CancellationTokenSource();
        var options = new ChangesFeedOptions
        {
            LongPoll = true,
            IncludeDocs = true,
            Since = _lastSeq
        };

        await foreach (var change in _workflowData.GetContinuousChangesAsync(options, null, tokenSource.Token))
        {
            if (change.Deleted || change.Document is null)
                continue;
            using var scope = _serviceProvider.CreateScope();
           
            try
            {
                var workflow = change.Document;
                var docId = workflow.Id;
                switch (workflow.WorkflowState)
                {
                    case EWorkflowDataStatus.PendingCleaning:
                        {
                            if (workflow.CurrentStepId.Count != 0) break;
                            var result = await ProcessStepGroupAsync(workflow, 0);
                            UpdateWorkflowStateByStepGroupResult(result.Item1, docId, result.Item2);
                            File.WriteAllText(_lastSeqTxtpath, change.Seq);
                            break;
                        }
                    case EWorkflowDataStatus.Cleaning:
                        {
                            var currentStepIds = workflow.CurrentStepId ?? new List<string>();

                            var currentSteps = workflow.Steps
                                .Where(s => currentStepIds.Contains(s.Step.StepId))
                                .ToList();



                            var isFailedAndObserved = currentSteps.Any(s => s.State.Equals(EStepDataStatus.Failed) && s.Observed);
                            if (isFailedAndObserved)
                            {
                                File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                UpdateWorkflowStateByStepGroupResult(StepGroupResult.Failed, docId, workflow.CurrentStepId);
                                break;
                            }

                            var isFailed = currentSteps.Any(s => s.State.Equals(EStepDataStatus.Failed));
                            if (isFailed)
                            {
                                break;
                            }

                            var allObserved = currentSteps
                                 .All(s => s.Observed);

                            if (!allObserved)
                                break;

                            var currentSequence = currentSteps.Select(s => s.Step.Sequence).Distinct().OrderByDescending(s => s).First();

                            var result = await ProcessStepGroupAsync(workflow, currentSequence);
                            UpdateWorkflowStateByStepGroupResult(result.Item1, docId, result.Item2);

                            File.WriteAllText(_lastSeqTxtpath, change.Seq);
                            break;
                        }
                    case EWorkflowDataStatus.Cleaned:
                        {
                            _workflowChannel.BasicAck(deliveryTag: workflow.DeliveryTag, multiple: false);
                            _logger.LogInformation("Cleaning workflow {WorkflowName} done.", workflow.WorkflowName);

                            var runtimeStateService = scope.ServiceProvider.GetRequiredService<IRuntimeStateService>();
                            await runtimeStateService.SetMaintenanceAsync(false);
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing workflow change: {Message}", ex.Message);
                //TODO: send Exception to Cloud
            }
           
        }
    }

    private CloudToDeviceMethod BuildDeviceMethod(string methodName, string? parameters, string docId, string stepId)
    {
        var method = new CloudToDeviceMethod(methodName, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));
        if (string.IsNullOrEmpty(parameters))
        {
            parameters = "{}";
        }

        var jsonNode = JsonNode.Parse(parameters).AsObject();
        jsonNode["docId"] = docId;
        jsonNode["stepId"] = stepId;

        method.SetPayloadJson(jsonNode.ToJsonString());

        return method;
    }

    public enum StepGroupResult { Running, Paused, Failed, Done }

    private async Task<Tuple<StepGroupResult, List<string>>> ProcessStepGroupAsync(WorkflowData workflow, int currentSequence)
    {
        var group = workflow.Steps
            .Where(s => s.State == EStepDataStatus.Pending && s.Step.Sequence > currentSequence)
            .GroupBy(s => s.Step.Sequence)
            .OrderBy(g => g.Key)
            .FirstOrDefault();

        if (group == null)
            return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Done, []);

        var stepList = group.ToList();
        var lockedDevices = new List<DeviceDocument>();

        stepList = stepList.Where(x => ExpressionHelper.EvaluateExpressionConditions(workflow, x.Step.Conditions)).ToList();
        var stepIds = stepList.Select(x => x.Step.StepId).ToList();

        foreach (var step in stepList)
        {
            var device = GetIdleDeviceForStep(step.Step.DeviceModelId, lockedDevices);
            if (device == null)
            {
                _logger.LogWarning("Not enough idle devices for step group of workflow {WorkflowName}. Pausing.", workflow.WorkflowName);
                return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Paused, stepIds);
            }
            step.Executor = device.DeviceId;
            lockedDevices.Add(device);
        }
        //! update current stepIds for running/ pending
        await UpdateWorkflowStateAsync(workflow, EWorkflowDataStatus.Cleaning, stepIds);

        foreach (var device in lockedDevices)
        {
            device.WorkingStatus = EWorkingStatus.Working;
            var option = new AddOrUpdateOptions { Rev = device.Rev };
            try
            {
                await _deviceData.AddOrUpdateAsync(device, options: option);
            }
            catch (CouchConflictException)
            {
                await UnlockDevicesAsync(lockedDevices);
                _logger.LogWarning("Race condition while locking devices. Pausing workflow.");
                return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Paused, stepIds);
            }
        }


        var invokeTasks = stepList.Select(async step =>
        {
            Console.WriteLine($"Invoke method {step.Step.Name} with sequence {step.Step.Sequence}");
            try
            {
                var methodInvoke = BuildDeviceMethod(step.Step.Function, step.Step.Parameters, docId: workflow.Id, stepId: step.Step.StepId);
                var response = await _serviceClient.InvokeDeviceMethodAsync(step.Executor, methodInvoke);
                return response.Status == 200;

                //return true;
            }
            catch (Exception)
            {
                return false;
            }
        });
        var invokeResults = await Task.WhenAll(invokeTasks);

        if (invokeResults.Any(r => !r))
        {
            var failedStep = stepList.Where((s, i) => !invokeResults[i]).ToList().First();
            var updateFailStepStateMsg = new UpdateStepStateMessages(workflow.Id, failedStep.Step.StepId, (int)EStepDataStatus.Failed);
            PublishMsgToUpdateWorkflowConsumer(updateFailStepStateMsg, nameof(UpdateStepStateMessages), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);

            await UnlockDevicesAsync(lockedDevices);
            _logger.LogError("Error while invoking device. Failing workflow.");
            return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Failed, stepIds);
        }

        _logger.LogInformation("Workflow {WorkflowName} executed. Running step group: {StepGroup}", workflow.WorkflowName, string.Join(", ", stepIds));
        return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Running, stepIds);
    }



    /// <summary>
    /// publish UpdateWorkflowStateMsg for updating workflow state, consuming by UpdateWorkflowConsumer
    /// </summary>
    /// <param name="stepGroupResult"></param>
    /// <param name="docId">Workflow Data docId</param>
    /// <param name="newState">EWorkflowDataStatus</param>
    /// <param name="currentStepIds"></param>
    private void UpdateWorkflowStateByStepGroupResult(StepGroupResult stepGroupResult, string docId, List<string> currentStepIds)
    {
        UpdateWorkflowStateMsg? msg = null;
        switch ((stepGroupResult))
        {
            case StepGroupResult.Done:
                msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Cleaned, CurrentIdList: [], IsComplete: true);
                break;
            case StepGroupResult.Paused:
                msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Cleaned, CurrentIdList: currentStepIds, IsComplete: false);
                break;
            case StepGroupResult.Failed:
                msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Cleaned, CurrentIdList: currentStepIds, IsComplete: false);
                break;
            default:
                break;
        }

        if (msg == null) return;

        PublishMsgToUpdateWorkflowConsumer(msg, nameof(UpdateWorkflowStateMsg), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
    }

    /// <summary>
    /// Method used for update workflow state in Pending or Running state. Make sure workflow 's step set Excecutor
    /// </summary>
    /// <param name="workflow"></param>
    /// <param name="newState"></param>
    /// <param name="currentStepIds"></param>
    /// <returns></returns>
    private async Task UpdateWorkflowStateAsync(WorkflowData workflow, EWorkflowDataStatus newState, List<string>? currentStepIds = null)
    {
        workflow.WorkflowState = newState;
        if (currentStepIds != null) workflow.CurrentStepId = currentStepIds;
        await _workflowData.AddOrUpdateAsync(workflow);
    }

    private void UpdateWorkflowState(string docId, EWorkflowDataStatus newState, List<string>? currentStepIds = null, bool isComplete = false)
    {
        var msg = new UpdateWorkflowStateMsg(docId, (int)newState, CurrentIdList: currentStepIds ?? [], isComplete);
        PublishMsgToUpdateWorkflowConsumer(msg, nameof(UpdateWorkflowStateMsg), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
    }

    private void PublishMsgToUpdateWorkflowConsumer<TMessage>(TMessage msg, string type, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

        var props = _stepAndWfChannel.CreateBasicProperties();
        props.Type = type;
        _stepAndWfChannel.BasicPublish(
            exchange: QueueConstants.EXCHANGE_NAME,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body
        );
    }

    private DeviceDocument? GetIdleDeviceForStep(string deviceModelId, List<DeviceDocument> lockedDevice)
    {
        var lockedDeviceIds = lockedDevice.Select(x => x.DeviceId).ToHashSet();

        var device = _deviceData
            .ToList()
            .Where(d => d.DeviceModelId == deviceModelId
                     && d.WorkingStatus == EWorkingStatus.Idle
                     && !lockedDeviceIds.Contains(d.DeviceId))
            .FirstOrDefault();

        return device;
    }

    private async Task<DeviceDocument?> UnLockDeviceAsync(string deviceId)
    {
        var device = _deviceData
            .ToList()
            .Where(d => d.DeviceId == deviceId && d.WorkingStatus == EWorkingStatus.Working)
            .FirstOrDefault();
        if (device == null) return null;
        device.WorkingStatus = EWorkingStatus.Idle;
        var option = new AddOrUpdateOptions { Rev = device.Rev }; // add rev to prevent conflict on unlock
        await _deviceData.AddOrUpdateAsync(device, options: option);

        return device;
    }

    private async Task UnlockDevicesAsync(List<DeviceDocument> devices)
    {
        foreach (var device in devices)
        {
            device.WorkingStatus = EWorkingStatus.Idle;
            var option = new AddOrUpdateOptions { Rev = device.Rev };
            try
            {
                await _deviceData.AddOrUpdateAsync(device, options: option);
            }
            catch (CouchConflictException)
            {
                continue;
            }
        }
    }


}
