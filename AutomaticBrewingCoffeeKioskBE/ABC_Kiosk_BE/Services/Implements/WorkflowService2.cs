using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using Services.Base;
using Services.Dtos.KioskMachine;
using Services.Interfaces;
using Shared.MessageStore;
using StackExchange.Redis;
using System.Text;
using static Domain.MessageRecords;

namespace Services.Implements
{
    public class WorkflowService2 : IWorkflowService2
    {
        private readonly ILogger<WorkflowService2> _logger;

        private readonly IConnection _conn;

        private readonly IDatabase _cacheDb;
        private readonly IRuntimeStateService _runtimeStateService;

        public WorkflowService2(ILogger<WorkflowService2> logger, IConnection connection, IDatabase cacheDb, IRuntimeStateService runtimeStateService)
        {
            _logger = logger;
            _conn = connection;
            _cacheDb = cacheDb;
            _runtimeStateService = runtimeStateService;
        }

        public async Task<BaseResult> ExecuteWorkflowAsync(ExecuteWorkflowDto dto)
        {
            if (!dto.IsValidSide())
                return new BaseResult
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Side must be 1 or 2"
                };
            var workflowList = new List<Workflow>();
            foreach (var wf in dto.WorkflowIds)
            {
                var workflow = await LoadWorkflowJson("Workflow", wf.WorkflowId);
                if (workflow == null)
                {
                    //remove order in cache
                    await _cacheDb.KeyDeleteAsync(dto.OrderId);
                    return new BaseResult
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"Product with ID {wf.WorkflowId} not found workflow."
                    };
                }
                ModifyStepValueBaseOnOption(ref workflow, wf.Options ?? []);
                workflowList.Add(workflow);
            }
            using var channel = _conn.CreateModel();
            //push all workflow to queue for execute-product-consumer
            var batch = channel.CreateBasicPublishBatch();
            workflowList.ForEach(w =>
            {
                var msg = new WorkflowExecuteMsg(dto.OrderId, dto.Side, w);
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
                batch.Add(exchange: QueueConstants.EXCHANGE_NAME,
                              routingKey: QueueConstants.QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY,
                              mandatory: true,
                              properties: null,
                              body: body);

            });
            batch.Publish();
            _logger.LogInformation("Published {Count} workflows successfully.", workflowList.Count);
            return new BaseResult { IsSuccess = true, StatusCode = 202, Message = "Success" };
        }

        private void ModifyStepValueBaseOnOption(ref Workflow workflow, List<StepOption> options)
        {
            foreach (var option in options)
            {
                var stepToModify = workflow.Steps.FirstOrDefault(s => s.DeviceModelId.Equals(option.DeviceModelId));
                if (stepToModify == null) continue;

                if (!string.IsNullOrEmpty(stepToModify.Parameters))
                {
                    var paramJObj = JObject.Parse(stepToModify.Parameters);
                    if (!paramJObj.ContainsKey(option.Target)) continue;

                    var oldValueStr = paramJObj[option.Target]?.ToString() ?? string.Empty;
                    if (!double.TryParse(oldValueStr, out var oldValue))
                    {
                        continue;
                    }
                    //value is percent
                    paramJObj[option.Target] = (int)(Math.Ceiling(oldValue * option.Value / 100));

                    //gán lại cho step
                    stepToModify.Parameters = paramJObj.ToString(Formatting.None);
                }
            }
        }

        /// <summary>
        /// Load workflow json from file system
        /// Execute Workflow: WorkflowId is ProductId
        /// Execute Clean Workflow: WorkflowId is WorkflowId
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        private async Task<Workflow?> LoadWorkflowJson(string folder, string workflowId)
        {
            //~\DataStorage\Workflow\{productId}.json
            string filePath = Path.Combine("DataStorage", folder, $"{workflowId}.json");
            if (!File.Exists(filePath)) return null;
            string json = await File.ReadAllTextAsync(filePath);
            if (json == null) return null;

            _logger.LogInformation("Get workflow successful");
            return JsonConvert.DeserializeObject<Workflow>(json);
        }

        public async Task<BaseResult> ExecuteCleanWorkflowAsync(ExecuteCleanWorkflowDto dto)
        {
            try
            {
                var workflow = await LoadWorkflowJson("Clean", dto.WorkflowId.ToString());
                if (workflow == null) return new BaseResult { IsSuccess = true, StatusCode = 404, Message = "Not found" };

                using var channel = _conn.CreateModel();
                //push all workflow to queue for execute-product-consumer
                var msg = new ExecuteCleanWorkflowMsg(workflow);

                var props = channel.CreateBasicProperties();
                props.Type = nameof(ExecuteCleanWorkflowMsg);
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));

                //set maintenance to true
                await _runtimeStateService.SetMaintenanceAsync(true);

                channel.BasicPublish(
                    exchange: QueueConstants.EXCHANGE_NAME,
                    routingKey: QueueConstants.QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY,
                    mandatory: true,
                    basicProperties: props,
                    body: body
                );
                _logger.LogInformation("Cleaning workflow executing");

                return new BaseResult { IsSuccess = true, StatusCode = 202, Message = "Success" };
            }
            catch (Exception e)
            {
                await _runtimeStateService.SetMaintenanceAsync(false);
                return new BaseResult { IsSuccess = false, StatusCode = 500, Message = e.Message };
            }

        }

        public async Task<BaseResult> GetAllCleanWorkflowsAsync()
        {
            try
            {
                List<CleanWorkflowDto> cleanWorkflows = new List<CleanWorkflowDto>();

                //get folder of clean workflow
                //~\DataStorage\Clean\{workflowId}.json
                string cleanFolderPath = Path.Combine("DataStorage", "Clean");

                if (!Directory.Exists(cleanFolderPath))
                {
                    return new BaseResult { IsSuccess = false, StatusCode = StatusCodes.Status409Conflict, Message = "Clean workflows not found. PLease conatct administratior." };
                }

                string[] filePaths = Directory.GetFiles(cleanFolderPath);

                foreach (string filePath in filePaths)
                {

                    try
                    {
                        string content = await File.ReadAllTextAsync(filePath);
                        var workflow = JsonConvert.DeserializeObject<Workflow>(content);
                        if (workflow == null)
                        {
                            _logger.LogWarning("Workflow in file {FilePath} is null or invalid.", filePath);
                            continue;
                        }
                        cleanWorkflows.Add(new CleanWorkflowDto
                        {
                            Id = workflow.WorkflowId,
                            Name = workflow?.Name ?? string.Empty,
                            Description = workflow?.Description ?? string.Empty
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return new BaseResult<List<CleanWorkflowDto>> { IsSuccess = true, StatusCode = 200, Message = "Success", ResponseRequest = cleanWorkflows };
            }
            catch (Exception e)
            {
                return new BaseResult { IsSuccess = false, StatusCode = 500, Message = e.Message };
            }
        }

        public async Task<BaseResult> GetAllWorkflowsAsync()
        {
            try
            {
                List<WorkflowDto> workflows = new List<WorkflowDto>();

                //get folder of clean workflow
                //~\DataStorage\Workflow\{workflowId}.json
                string cleanFolderPath = Path.Combine("DataStorage", "Workflow");

                if (!Directory.Exists(cleanFolderPath))
                {
                    return new BaseResult { IsSuccess = false, StatusCode = StatusCodes.Status409Conflict, Message = "Workflows not found. PLease conatct administratior." };
                }

                string[] filePaths = Directory.GetFiles(cleanFolderPath);

                foreach (string filePath in filePaths)
                {

                    try
                    {
                        string content = await File.ReadAllTextAsync(filePath);
                        var workflow = JsonConvert.DeserializeObject<Workflow>(content);
                        if (workflow == null)
                        {
                            _logger.LogWarning("Workflow in file {FilePath} is null or invalid.", filePath);
                            continue;
                        }
                        workflows.Add(new WorkflowDto
                        {
                            Id = workflow.ProductId!,
                            Name = workflow?.Name ?? string.Empty,
                            Description = workflow?.Description ?? string.Empty
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return new BaseResult<List<WorkflowDto>> { IsSuccess = true, StatusCode = 200, Message = "Success", ResponseRequest = workflows };
            }
            catch (Exception e)
            {
                return new BaseResult { IsSuccess = false, StatusCode = 500, Message = e.Message };
            }
        }
    }
}
