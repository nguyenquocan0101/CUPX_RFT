
using CouchDb.Domain.Enums;
using CouchDB.Driver;
using CouchDB.Driver.ChangesFeed;
using Domain.CouchDbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.MessageStore;
using Newtonsoft.Json;
using System.Text;
using static Domain.MessageRecords;


namespace Services.Background
{
    public class StepObserverWorker : BackgroundService
    {
        private readonly CouchClient _couchClient;
        private readonly ICouchDatabase<WorkflowData> _workflowData = null!; // Initialized in the constructor
        private readonly ICouchDatabase<DeviceDocument> _deviceData = null!; // Initialized in the constructor

        private readonly ILogger<StepObserverWorker> _logger;
        private readonly string _lastSeqTxtpath = string.Empty;
        private readonly string _lastSeq;

        //private readonly RabbitMQ.Client.IModel _deviceChannel;
        private readonly RabbitMQ.Client.IModel _stepChannel;


        public StepObserverWorker([FromKeyedServices(QueueConstants.QUEUE_STEP_UPDATE)] RabbitMQ.Client.IModel stepChannel,
            [FromKeyedServices(QueueConstants.QUEUE_DEVICE_UPDATE)] RabbitMQ.Client.IModel deviceChannel,
            IConfiguration configuration, ILogger<StepObserverWorker> logger)
        {
            _logger = logger;
            var url = configuration["CouchDB:Url"]!;
            var username = configuration["CouchDB:Username"]!;
            var pwd = configuration["CouchDB:Pwd"]!;
            _couchClient = new CouchClient(url,
                builder => builder.UseBasicAuthentication(username, pwd));

            _workflowData = _couchClient.GetOrCreateDatabaseAsync<WorkflowData>("workflowdatas").Result;
            _deviceData = _couchClient.GetOrCreateDatabaseAsync<DeviceDocument>("devicedocuments").Result;

            _lastSeqTxtpath = Path.Combine(Directory.GetCurrentDirectory(), "workflow_seq.txt");
            _lastSeq = File.Exists(_lastSeqTxtpath) ? File.ReadAllText(_lastSeqTxtpath) : "0";
            if (!File.Exists(_lastSeqTxtpath))
                File.WriteAllText(_lastSeqTxtpath, _lastSeq);

            //_deviceChannel = deviceChannel;
            _stepChannel = stepChannel;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
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

                if (change.Document.WorkflowState.Equals(EWorkflowDataStatus.Running) || change.Document.WorkflowState.Equals(EWorkflowDataStatus.Cleaning))
                {
                    try
                    {
                        var stepCheckList = new List<StepData>();
                        var workflow = change.Document;
                        var currentStepIdList = workflow.CurrentStepId;
                        if (currentStepIdList == null || currentStepIdList.Count == 0) continue;

                        foreach (var currentStepId in currentStepIdList)
                        {
                            var targetStep = change.Document.Steps.First(s => s.Step.StepId.Equals(currentStepId));
                            if (targetStep.Observed) continue;

                            var targetIndex = workflow.Steps.FindIndex(s => s.Step.StepId.Equals(currentStepId));
                            switch (targetStep.State)
                            {
                                case EStepDataStatus.Done:
                                    _logger.LogWarning("Step {0} done. Continue.", targetStep.Step.Name);
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    PublishUnlockMessage(workflow.Steps[targetIndex].Executor);
                                    PublishUpdateStepObservedMessage(workflow.Id, targetStep.Step.StepId, true);
                                    break;

                                case EStepDataStatus.Failed:
                                    _logger.LogWarning("Step {0} failed. Change workflow state to fail workflow.", targetStep.Step.Name);
                                    File.WriteAllText(_lastSeqTxtpath, change.Seq);
                                    //PublishUnlockMessage(workflow.Steps[targetIndex].Executor);

                                    var msg = $"Workflow {workflow.WorkflowName} failed at step {targetStep.Step.Sequence}: {targetStep.Step.Name}.";
                                    PublishUpdateStepObservedMessage(workflow.Id, targetStep.Step.StepId, true, msg);
                                    break;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing workflow change: {Message}", ex.Message);
                    }
                }
                
            }
        }

        private void PublishUnlockMessage(string deviceId)
        {
            var msg = new UnlockDeviceMsg(deviceId);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

            //var props = _deviceChannel.CreateBasicProperties();
            //props.Type = nameof(UnlockDeviceMsg);
            //_deviceChannel.BasicPublish(
            //    exchange: QueueConstants.EXCHANGE_NAME,
            //    routingKey: QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY,
            //    mandatory: true,
            //    basicProperties: props,
            //    body: body
            //);

            //đẩy chung vào step-update để device luôn dc unlock trước khi step observed
            var props = _stepChannel.CreateBasicProperties();
            props.Type = nameof(UnlockDeviceMsg);
            _stepChannel.BasicPublish(
                exchange: QueueConstants.EXCHANGE_NAME,
                routingKey: QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY,
                mandatory: true,
                basicProperties: props,
                body: body
            );
        }

        private void PublishUpdateStepObservedMessage(string docId, string stepId, bool observed, string? message = null)
        {
            var msg = new UpdateStepObservedMsg(docId, stepId, observed, message);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

            var props = _stepChannel.CreateBasicProperties();
            props.Type = nameof(UpdateStepObservedMsg);
            _stepChannel.BasicPublish(
                exchange: QueueConstants.EXCHANGE_NAME,
                routingKey: QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY,
                mandatory: true,
                basicProperties: props,
                body: body
            );
        }

    }
}
