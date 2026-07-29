using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Step;
using Services.Dtos.Workflow;

namespace Services.Interfaces
{
    public interface IWorkflowService
    {
        // Workflow
        Task<BaseResult<WorkflowQueryDto, Paginate<WorkflowDto>>> GetWorkflows(WorkflowQueryDto workflowQueryDto);
        Task<BaseResult<CreateWorkflowDto, WorkflowDto>> CreateWorkflow(CreateWorkflowDto createWorkflowDto);
        Task<BaseResult<string, WorkflowDto>> GetWorkflow(string workflowId);
        Task<BaseResult<string, WorkflowDto>> UpdateWorkflow(string workflowId, UpdateWorkflowDto updateWorkflowDto);
        Task<BaseResult<string, WorkflowDto>> RemoveWorkflow(string workflowId);


        // Step
        Task<BaseResult<CreateStepDto, StepDto>> CreateStep(CreateStepDto createStepDto);
        Task<BaseResult<UpdateStepDto, StepDto>> UpdateStep(string stepId, UpdateStepDto updateStepDto);
        Task<BaseResult<string, StepDto>> RemoveStep(string stepId);

        Task<BaseResult<ReorderStepDto, List<StepDto>>> ReorderStep(string workflowId, ReorderStepDto reorderStepDto);
    }
}