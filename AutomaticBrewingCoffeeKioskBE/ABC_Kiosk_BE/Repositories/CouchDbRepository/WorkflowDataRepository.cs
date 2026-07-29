using CouchDb.Domain.Enums;
using CouchDB.Driver.Extensions;
using Domain.CouchDbModels;
using Domain.Models;

namespace Repositories.CouchDbRepository
{

    public interface IWorkflowDataRepository
    {
        Task<WorkflowData?> GetbyDocIdAsync(string docId);
        Task UpdateWorkflowDataAsync(WorkflowData workflowData);
        Task AddFromWorkflowAsync(Workflow workflow, ulong deliveryTag, int side, string? orderId = null);
        Task AddFromCleanWorkflowAsync(Workflow workflow, ulong deliveryTag);
        Task<bool> UpdateStepInWorkflowAsync(string docId, string stepId, int state);
    }   
    public class WorkflowDataRepository : IWorkflowDataRepository
    {
        private readonly KioskDbContext _context;
        public WorkflowDataRepository(KioskDbContext context)
        {
            _context = context;
        }

        public async Task<WorkflowData?> GetbyDocIdAsync(string docId)
        {
            var workflow = await _context.WorkflowDatas.FindAsync(docId);
            return workflow;
        }

        public async Task UpdateWorkflowDataAsync(WorkflowData workflowData)
        {
            await _context.WorkflowDatas.AddOrUpdateAsync(workflowData);
        }

        public async Task AddFromWorkflowAsync(Workflow workflow, ulong deliveryTag, int side, string? orderId = null)
        {
            var workflowData = new WorkflowData();
            workflowData.WorkflowId = workflow.WorkflowId;
            workflowData.DeliveryTag = deliveryTag;
            workflowData.CurrentStepId = [];
            workflowData.WorkflowName = workflow.Name;
            workflowData.WorkflowState = EWorkflowDataStatus.Pending;
            workflowData.IsCompleted = false;
            workflowData.Message = string.Empty;
            workflowData.ProductId = workflow.ProductId;
            workflowData.OrderId = orderId;
            workflowData.Side = side;
            workflowData.Steps = [.. workflow.Steps
                .Select(step => new StepData
                {
                    Step = step,
                    State = EStepDataStatus.Pending,
                })
                .OrderBy(step => step.Step.Sequence)];

            await _context.WorkflowDatas.AddAsync(workflowData);
        }

        public async Task<bool> UpdateStepInWorkflowAsync(string docId, string stepId, int state)
        {
            var db = _context.WorkflowDatas;
            var targetWorkflow = await db.FindAsync(docId);
            if (targetWorkflow == null)
                return false;

            var targetStep = targetWorkflow.Steps.FirstOrDefault(s => s.Step.StepId == stepId);
            if (targetStep == null)
                return false;

            targetStep.State = (EStepDataStatus)state;
            
            await db.AddOrUpdateAsync(targetWorkflow);
            return true;
        }

        public async Task AddFromCleanWorkflowAsync(Workflow workflow, ulong deliveryTag)
        {
            var workflowData = new WorkflowData();
            workflowData.WorkflowId = workflow.WorkflowId;
            workflowData.DeliveryTag = deliveryTag;
            workflowData.CurrentStepId = [];
            workflowData.WorkflowName = workflow.Name;
            workflowData.WorkflowState = EWorkflowDataStatus.PendingCleaning;
            workflowData.IsCompleted = false;
            workflowData.Message = string.Empty;
            workflowData.Steps = [.. workflow.Steps
                .Select(step => new StepData
                {
                    Step = step,
                    State = EStepDataStatus.Pending,
                })
                .OrderBy(step => step.Step.Sequence)];

            await _context.WorkflowDatas.AddAsync(workflowData);
        }
    }
}
