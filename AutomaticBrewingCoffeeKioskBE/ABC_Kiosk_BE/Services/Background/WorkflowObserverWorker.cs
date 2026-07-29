using CouchDB.Driver.ChangesFeed;
using CouchDB.Driver;
using Domain.CouchDbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Services.Utils;
using CouchDB.Driver.DatabaseApiMethodOptions;
using CouchDB.Driver.Exceptions;
using CouchDb.Domain.Enums;
using RabbitMQ.Client;
using Shared.MessageStore;
using static Domain.MessageRecords;
using System.Text;
using System.Text.Json;
using Services.Interfaces;


public class WorkflowObserverWorker : BackgroundService
{
    private readonly CouchClient _couchClient;
    private readonly ICouchDatabase<WorkflowData> _workflowData;
    private readonly ICouchDatabase<DeviceDocument> _deviceData;
    private readonly ILogger<WorkflowObserverWorker> _logger;
    private readonly string _lastSeqTxtpath;
    private string _lastSeq;
    private readonly IDeviceMethodInvoker _deviceMethodInvoker;
    private readonly IWorkflowDeliveryTracker _deliveryTracker;
    private readonly IServiceProvider _serviceProvider;

    private readonly IModel _workflowChannel;
    private readonly IModel _orderChannel;
    private readonly IModel _stepAndWfChannel;

    public WorkflowObserverWorker(IServiceProvider provider, IConfiguration configuration,
        ILogger<WorkflowObserverWorker> logger, [FromKeyedServices(QueueConstants.QUEUE_WORKFLOW_EXECUTE)] IModel workflowChannel,
        [FromKeyedServices(QueueConstants.QUEUE_WORKFLOW_EXECUTE)] IModel orderChannel,
        [FromKeyedServices(QueueConstants.QUEUE_STEP_UPDATE)] IModel stepAndWfChannel,
        IDeviceMethodInvoker deviceMethodInvoker,
        IWorkflowDeliveryTracker deliveryTracker)
    {
        _logger = logger;
        var url = configuration["CouchDB:Url"]!;
        var username = configuration["CouchDB:Username"]!;
        var pwd = configuration["CouchDB:Pwd"]!;
        _couchClient = new CouchClient(url,
            builder => builder.UseBasicAuthentication(username, pwd));

        _workflowData = _couchClient.GetOrCreateDatabaseAsync<WorkflowData>(StringHelper.GetCouchDbDatabaseNameFormat(nameof(WorkflowData))).Result;
        _deviceData = _couchClient.GetOrCreateDatabaseAsync<DeviceDocument>(StringHelper.GetCouchDbDatabaseNameFormat(nameof(DeviceDocument))).Result;

        _lastSeqTxtpath = Path.Combine(Directory.GetCurrentDirectory(), "workflow_seq.txt");
        _lastSeq = File.Exists(_lastSeqTxtpath) ? File.ReadAllText(_lastSeqTxtpath) : "0";
        if (!File.Exists(_lastSeqTxtpath))
            File.WriteAllText(_lastSeqTxtpath, _lastSeq);

        _deviceMethodInvoker = deviceMethodInvoker;
        _deliveryTracker = deliveryTracker;
        _serviceProvider = provider;

        _workflowChannel = workflowChannel;
        _orderChannel = orderChannel;
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
                    case EWorkflowDataStatus.Pending:
                        {
                            if (workflow.CurrentStepId.Count != 0) break;
                            var result = await ProcessStepGroupAsync(workflow, 0);
                            UpdateWorkflowStateByStepGroupResult(result.Item1, docId, result.Item2);
                            File.WriteAllText(_lastSeqTxtpath, change.Seq);
                            break;
                        }
                    case EWorkflowDataStatus.Running:
                        {
                            var currentStepIds = workflow.CurrentStepId ?? new List<string>();

                            var currentSteps = workflow.Steps
                                .Where(s => currentStepIds.Contains(s.Step.StepId))
                                .ToList();



                            var isFailedAndObserved = currentSteps.Any(s => s.State.Equals(EStepDataStatus.Failed) && s.Observed);
                            if (isFailedAndObserved)
                            {
                                File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                UpdateWorkflowState(docId, EWorkflowDataStatus.Failed);
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
                    case EWorkflowDataStatus.Pause:
                        {
                            // Pause usually occurs when a device is locked or not enough idle devices are available.
                            await CleanupWorkflowResources(workflow);
                            File.WriteAllText(_lastSeqTxtpath, change.Seq);
                            UpdateWorkflowState(docId, EWorkflowDataStatus.Reseting);
                            break;
                        }

                    case EWorkflowDataStatus.Failed:
                        {
                            // Failure occurs when device method invocation fails or step state is returned as Fail (due to timeout or execution error).
                            _logger.LogInformation("Failed workflow {WorkflowName}", workflow.WorkflowName);
                            _logger.LogInformation("Clean resource for reseting workflow {WorkflowName}", workflow.WorkflowName);
                            await CleanupWorkflowResources(workflow);
                            _logger.LogInformation("Done cleaning. Start reseting");
                            File.WriteAllText(_lastSeqTxtpath, change.Seq);
                            UpdateWorkflowState(docId, EWorkflowDataStatus.Reseting);

                            break;
                        }

                    case EWorkflowDataStatus.Done:
                        {
                            try
                            {
                                PublishFinishProductMessage(workflow.OrderId, workflow.ProductId);
                                _logger.LogInformation("Pushed message. Order {OrderId} is ready for product check.", workflow.OrderId);
                                AckWorkflowDelivery(workflow.DeliveryTag);
                                _logger.LogInformation("Completed workflow {WorkflowName}", workflow.WorkflowName);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError("Error when completing: {e}", e.Message);
                              
                            }
                            break;
                        }

                    case EWorkflowDataStatus.Reseting:
                        {
                            //check current failed step Id list -> Get callback Step 
                            var currentStepIdsForReseting = workflow.CurrentStepId ?? new List<string>();

                            //this step is fail in running
                            var failCurrentStep = workflow.Steps
                              .Where(s => currentStepIdsForReseting.Contains(s.Step.StepId) && s.State == EStepDataStatus.Failed && !s.IsRunCallBack)
                              .OrderBy(s => s.Step.Sequence)
                              .Distinct()
                              .FirstOrDefault();

                            //implement call back by invoke next step
                            if (failCurrentStep != null)
                            {
                                var callbackStepForFailedCurrentStep = workflow.Steps
                                   .OrderBy(s => s.Step.Sequence)
                                   .FirstOrDefault(s => !string.IsNullOrEmpty(failCurrentStep.Step.CallbackStepCode) && s.Step.StepCode == failCurrentStep.Step.CallbackStepCode);

                                //no more workflow -> change state to Reseted   
                                if (callbackStepForFailedCurrentStep == null)
                                {
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    _logger.LogInformation("No step executed. Done reseting");
                                    UpdateWorkflowState(docId, EWorkflowDataStatus.Reseted);
                                    break;
                                }

                                var device = GetIdleDeviceForStep(callbackStepForFailedCurrentStep.Step.DeviceModelId, []);
                                if (device == null)
                                {
                                    _logger.LogInformation("No device for reseting workflow. Workflow reset aborted due to conflict. Notifying cloud...");
                                    
                                    // TODO: call cloud to alert reseting fail
                                    return;
                                }
                                var setReadyCallbackMsg = new SetReadyCallbackMsg(docId, callbackStepForFailedCurrentStep.Step.StepId, device.DeviceId);
                                PublishMsgToUpdateWorkflowConsumer(setReadyCallbackMsg, nameof(SetReadyCallbackMsg), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
                                await ProcessCallbackStep(workflow, callbackStepForFailedCurrentStep, device);

                                File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                break;
                            }

                            //this step is callback step processing in Reseting
                            var callbackCurrentStep = workflow.Steps
                             .Where(s => currentStepIdsForReseting.Contains(s.Step.StepId))
                             .OrderBy(s => s.Step.Sequence)
                             .Distinct()
                             .First();
                            if (!callbackCurrentStep.IsRunCallBack)
                            {
                                
                                //set ready for next step
                                var nextCallbackStep = workflow.Steps
                                  .OrderBy(s => s.Step.Sequence)
                                  .FirstOrDefault(s =>
                                      !string.IsNullOrEmpty(s.Step.StepCode) &&
                                      s.Step.StepCode == callbackCurrentStep.Step.CallbackStepCode);

                                //no more workflow -> change state to Reseted   
                                if (nextCallbackStep == null)
                                {
                                    UpdateWorkflowState(docId, EWorkflowDataStatus.Reseted);
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    break;
                                }

                                var device = GetIdleDeviceForStep(nextCallbackStep.Step.DeviceModelId, []);
                                if (device == null)
                                {
                                    _logger.LogInformation("No device for reseting workflow. Workflow reset aborted due to conflict. Notifying cloud...");
                                    // TODO: call cloud to alert reseting fail
                                    return;
                                }
                                var msg = new SetReadyCallbackMsg(docId, nextCallbackStep.Step.StepId, device.DeviceId);
                                PublishMsgToUpdateWorkflowConsumer(msg, nameof(SetReadyCallbackMsg), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
                                await ProcessCallbackStep(workflow, nextCallbackStep, device);
                                File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                break;
                            }
                            else
                            {
                                if (!callbackCurrentStep.CallbackObserved)
                                {
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    break;
                                }
                                //handle fail callback step
                                if (callbackCurrentStep.State == EStepDataStatus.Failed)
                                {
                                    _logger.LogError("Callback Step {0} failed. Notify to Cloud....", callbackCurrentStep.Step.Name);
                                    AckWorkflowDelivery(workflow.DeliveryTag);
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    break;
                                }
                                var msg = new ResetForNextCallbackStepMsg(docId, callbackCurrentStep.Step.StepId);
                                PublishMsgToUpdateWorkflowConsumer(msg, nameof(ResetForNextCallbackStepMsg), QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
                                File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                break;
                            }
                        }
                    case EWorkflowDataStatus.Reseted:
                        {
                            try
                            {
                                PublishFailProductMessage(workflow.OrderId, workflow.ProductId, workflow.Message);
                                _logger.LogInformation("Pushed message. Order {OrderId} is ready for updating fail status.", workflow.OrderId);
                                AckWorkflowDelivery(workflow.DeliveryTag);
                                _logger.LogInformation("Reseted workflow {WorkflowName}", workflow.WorkflowName);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError("Error when reseted: {e}",e.Message);
                            }

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

    private static Dictionary<string, string> ParseParameters(string? parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters))
            return new();

        try
        {
            using var document = JsonDocument.Parse(parameters);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                return document.RootElement.EnumerateObject()
                    .ToDictionary(property => property.Name, property => property.Value.GetRawText());
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Preserve malformed legacy payloads for the device error path.
        }

        return new Dictionary<string, string> { ["raw"] = parameters };
    }

    private static DeviceCommandRequest CreateDeviceCommand(
        string workflowId,
        string stepId,
        string deviceId,
        string method,
        string? parameters,
        string suffix = "")
    {
        var commandId = $"{workflowId}:{stepId}{suffix}";
        return new DeviceCommandRequest(
            commandId,
            1,
            commandId,
            workflowId,
            stepId,
            deviceId,
            method,
            ParseParameters(parameters),
            DateTimeOffset.UtcNow,
            30000);
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
        await UpdateWorkflowStateAsync(workflow, EWorkflowDataStatus.Running, stepIds);

        //TODO: change to push message
        //UpdateWorkflowState(docId, );

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
                var request = CreateDeviceCommand(workflow.Id, step.Step.StepId, step.Executor, step.Step.Function, step.Step.Parameters);
                var response = await _deviceMethodInvoker.InvokeAsync(request);
                return response.Status == "Completed";

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

            //await UnlockDevicesAsync(lockedDevices);
            _logger.LogError("Error while invoking device. Failing workflow.");
            return new Tuple<StepGroupResult, List<string>>(StepGroupResult.Failed, stepIds);
        }

        foreach (var step in stepList)
        {
            var updateStepStateMsg = new UpdateStepStateMessages(
                workflow.Id,
                step.Step.StepId,
                (int)EStepDataStatus.Done);
            PublishMsgToUpdateWorkflowConsumer(
                updateStepStateMsg,
                nameof(UpdateStepStateMessages),
                QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
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
                msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Done, CurrentIdList: [], IsComplete: true);
                break;
            case StepGroupResult.Paused:
                msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Pause, CurrentIdList: currentStepIds, IsComplete: true);
                break;
            //case StepGroupResult.Failed:
            //    msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Failed, CurrentIdList: [], IsComplete: false);
            //    break;
            //case StepGroupResult.Running:
            //    msg = new UpdateWorkflowStateMsg(docId, (int)EWorkflowDataStatus.Running, CurrentIdList: currentStepIds, IsComplete: false);
            //    break;
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

    private void PublishFailProductMessage(string orderId, string productId, string message)
    {
        var msg = new FailProductMsg(orderId, productId, DateTime.Now.ToString(), message);
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

        var props = _orderChannel.CreateBasicProperties();
        props.Type = nameof(FailProductMsg);
        _orderChannel.BasicPublish(
            exchange: QueueConstants.EXCHANGE_NAME,
            routingKey: QueueConstants.QUEUE_ORDER_ROUTING_KEY_UPDATE,
            mandatory: true,
            basicProperties: props,
            body: body
        );
    }

    private void PublishFinishProductMessage(string orderId, string productId)
    {
        var msg = new FinishProductMsg(orderId, productId, DateTime.Now.ToString());
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

        var props = _orderChannel.CreateBasicProperties();
        props.Type = nameof(FinishProductMsg);
        _orderChannel.BasicPublish(
            exchange: QueueConstants.EXCHANGE_NAME,
            routingKey: QueueConstants.QUEUE_ORDER_ROUTING_KEY_UPDATE,
            mandatory: true,
            basicProperties: props,
            body: body
        );
    }

    private async Task CleanupWorkflowResources(WorkflowData workflow)
    {
        var stepsWithExecutor = workflow.Steps
            .Where(s => !string.IsNullOrEmpty(s.Executor))
            .ToList();
        var unlockTasks = stepsWithExecutor
        .Select(x => UnLockDeviceAsync(x.Executor));

        await Task.WhenAll(unlockTasks);
    }

    private void AckWorkflowDelivery(ulong deliveryTag)
    {
        if (!_deliveryTracker.TryTake(deliveryTag))
        {
            _logger.LogWarning("Skipping stale workflow delivery tag {DeliveryTag} after restart.", deliveryTag);
            return;
        }

        _workflowChannel.BasicAck(deliveryTag, multiple: false);
    }

    private async Task ProcessCallbackStep(WorkflowData workflow, StepData callbackStep, DeviceDocument device)
    {

        // Update device state to Working
        device.WorkingStatus = EWorkingStatus.Working;
        var option = new AddOrUpdateOptions { Rev = device.Rev };
        try
        {
            _deviceData.AddOrUpdateAsync(device, options: option).Wait();
        }
        catch (CouchConflictException)
        {
            // Rollback device state if there is a race condition
            await UnLockDeviceAsync(device.DeviceId);
            _logger.LogWarning("Race condition while locking device. Workflow reset aborted due to conflict. Notifying cloud...");
            // TODO: call cloud to alert reseting fail
            return;
        }

        //Invoke the step
        var iotHubCallOk = false;

        try
        {
            Console.WriteLine($"Invoke method {callbackStep.Step.Name} with sequence {callbackStep.Step.Sequence} for reseting");
            var request = CreateDeviceCommand(workflow.Id, callbackStep.Step.StepId, device.DeviceId, callbackStep.Step.Function, callbackStep.Step.Parameters, ":callback");
            var response = await _deviceMethodInvoker.InvokeAsync(request);
            iotHubCallOk = response.Status == "Completed";
            Console.WriteLine($"Invoke result: {response.Status}. {response.ErrorMessage}");
            //iotHubCallOk = true;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while invoking device method for callback step {StepName} in workflow {WorkflowName}. {e}", callbackStep.Step.Name, workflow.WorkflowName, e);
            iotHubCallOk = false;
        }

        if (!iotHubCallOk)
        {
            await UnLockDeviceAsync(device.DeviceId);
            _logger.LogError("Resetting workflow failed. Notifying cloud...");
            // TODO: call cloud to alert reseting fail
            return;
        }
    }

}
