using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.MessageStore;
using Newtonsoft.Json.Linq;
using Repositories.CouchDbRepository;
using static Domain.MessageRecords;
using CouchDb.Domain.Enums;

namespace Kiosk.ApiService.Consumers
{
    public class UpdateWorkflowConsumer : BackgroundService, IDisposable
    {
        private readonly ILogger<UpdateWorkflowConsumer> _logger;
        private IModel _channel;
        private readonly IServiceProvider _provider;

        public UpdateWorkflowConsumer(IServiceProvider provider, ILogger<UpdateWorkflowConsumer> logger, [FromKeyedServices(QueueConstants.QUEUE_STEP_UPDATE)] IModel channel)
        {
            _logger = logger;
            _channel = channel;
            _provider = provider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming(QueueConstants.QUEUE_STEP_UPDATE, stoppingToken);
            await Task.CompletedTask;
        }

        private void StartConsuming(string queueName, CancellationToken cancellationToken)
        {
            //just consume the queue if it exists
            try
            {
                _channel.QueueDeclarePassive(queue: queueName);
            }
            catch (OperationInterruptedException)
            {
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    string type = ea.BasicProperties.Type;
                    await UpdateBasedOnMsgAsync(message, type);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer); //khởi động quá trình lắng nghe

        }

        private async Task<bool> UpdateBasedOnMsgAsync(string message, string messageType)
        {
            try
            {
                var jObj = JsonConvert.DeserializeObject<JObject>(message);
                await using var scope = _provider.CreateAsyncScope();
                var workflowDataRepo = scope.ServiceProvider.GetRequiredService<IWorkflowDataRepository>();
                var deviceDocRepo = scope.ServiceProvider.GetRequiredService<IDeviceDocumentRepository>();
                switch (messageType)
                {
                    case nameof(UpdateStepObservedMsg):
                        var obsMsg = jObj.ToObject<UpdateStepObservedMsg>();
                        var workflow = await workflowDataRepo.GetbyDocIdAsync(obsMsg.DocId);
                        if (workflow == null)
                        {
                            _logger.LogWarning("Workflow not found with ID: {DocId}", obsMsg.DocId);
                            break;
                        }

                        if (!string.IsNullOrEmpty(obsMsg.Message))
                            workflow.Message = obsMsg.Message;

                        var targetStep = workflow.Steps.First(s => s.Step.StepId == obsMsg.StepId);
                        if (targetStep == null)
                        {
                            _logger.LogWarning("Step not found in workflow. StepId: {StepId}", obsMsg.StepId);
                            break;
                        }
                        targetStep.Observed = obsMsg.Observed;

                        await workflowDataRepo.UpdateWorkflowDataAsync(workflow);
                        _logger.LogInformation("Marked step {StepId} as observed={Observed} in workflow {WorkflowId}", obsMsg.StepId, obsMsg.Observed, obsMsg.DocId);
                        break;
                    case nameof(UpdateCallbackStepObservedMsg):
                        var callbackObsMsg = jObj.ToObject<UpdateStepObservedMsg>();
                        var resetingWorkflow = await workflowDataRepo.GetbyDocIdAsync(callbackObsMsg.DocId);
                        if (resetingWorkflow == null)
                        {
                            _logger.LogWarning("Workflow not found with ID: {DocId}", callbackObsMsg.DocId);
                            break;
                        }

                        if (!string.IsNullOrEmpty(callbackObsMsg.Message))
                            resetingWorkflow.Message = callbackObsMsg.Message;

                        var callbackStep = resetingWorkflow.Steps.First(s => s.Step.StepId == callbackObsMsg.StepId);
                        if (callbackStep == null)
                        {
                            _logger.LogWarning("Step not found in workflow. StepId: {StepId}", callbackObsMsg.StepId);
                            break;
                        }
                        callbackStep.CallbackObserved = callbackObsMsg.Observed;

                        await workflowDataRepo.UpdateWorkflowDataAsync(resetingWorkflow);
                        _logger.LogInformation("Marked callback step {StepId} as observed={Observed} in workflow {WorkflowId}", callbackObsMsg.StepId, callbackObsMsg.Observed, callbackObsMsg.DocId);
                        break;

                    case nameof(UpdateStepStateMessages):
                        var stateMsg = jObj.ToObject<UpdateStepStateMessages>();
                        await workflowDataRepo.UpdateStepInWorkflowAsync(stateMsg.DocId, stateMsg.StepId, stateMsg.State);
                        break;
                    //Thêm logic check unlock msg
                    case nameof(UnlockDeviceMsg):
                        var unlockMsg = jObj.ToObject<UnlockDeviceMsg>();
                        await deviceDocRepo.UnlockDeviceDocAsync(unlockMsg.DeviceId);
                        break;
                    case nameof(UpdateWorkflowStateMsg):
                        {
                            var updateWorkflowStateMsg = jObj.ToObject<UpdateWorkflowStateMsg>();
                            var targetWorkflow = await workflowDataRepo.GetbyDocIdAsync(updateWorkflowStateMsg.DocId);

                            if (targetWorkflow.WorkflowState != (EWorkflowDataStatus)updateWorkflowStateMsg.NewSate)
                            {
                                Console.WriteLine($"Update workflow {targetWorkflow.WorkflowId} to {(EWorkflowDataStatus)updateWorkflowStateMsg.NewSate}");
                                targetWorkflow.WorkflowState = (EWorkflowDataStatus)updateWorkflowStateMsg.NewSate;
                            }
                            if (updateWorkflowStateMsg.CurrentIdList.Count > 0)
                            {
                                targetWorkflow.CurrentStepId = updateWorkflowStateMsg.CurrentIdList;
                            }
                            targetWorkflow.IsCompleted = updateWorkflowStateMsg.IsComplete;
                            await workflowDataRepo.UpdateWorkflowDataAsync(targetWorkflow);
                            break;
                        }
                    case nameof(SetReadyCallbackMsg):
                        {
                            var setReadyCallbackMsg = jObj.ToObject<SetReadyCallbackMsg>();
                            var targetWorkflow = await workflowDataRepo.GetbyDocIdAsync(setReadyCallbackMsg.DocId);
                            var stepToUpdate = targetWorkflow.Steps.FirstOrDefault(s => s.Step.StepId == setReadyCallbackMsg.StepId);
                            targetWorkflow.CurrentStepId = [stepToUpdate.Step.StepId];
                            if (stepToUpdate != null)
                            {
                                stepToUpdate.Executor = setReadyCallbackMsg.Executor;
                                stepToUpdate.IsRunCallBack = true;
                                stepToUpdate.CallbackObserved = false;
                                stepToUpdate.State = EStepDataStatus.Pending;
                                await workflowDataRepo.UpdateWorkflowDataAsync(targetWorkflow);
                            }
                            break;
                        }
                    case nameof(ResetForNextCallbackStepMsg):
                        {

                            var resetForNextCallbackStepMsg = jObj.ToObject<ResetForNextCallbackStepMsg>();
                            var targetWorkflow = await workflowDataRepo.GetbyDocIdAsync(resetForNextCallbackStepMsg.DocId);
                            var currentStep = targetWorkflow.Steps.FirstOrDefault(s => s.Step.StepId == resetForNextCallbackStepMsg.CurrentStepId);
                            if (currentStep != null)
                            {
                                currentStep.IsRunCallBack = false;
                            }

                            await workflowDataRepo.UpdateWorkflowDataAsync(targetWorkflow);
                            break;
                        }

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }
    }
}
