using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Base;
using Services.Dtos.Step;
using Services.Dtos.Workflow;

namespace Services.Interfaces
{
    public interface IWorkflowService
    {
        // Workflow
        // Task<BaseResult<IEnumerable<WorkflowDto>>> GetWorkflows(WorkflowQueryDto workflowQueryDto);
        Task<BaseResult<WorkflowDto>> GetWorkflow(string workflowId);
        Task<BaseResult<CreateWorkflowDto, WorkflowDto>> CreateWorkflow(CreateWorkflowDto createWorkflowDto);
        Task<BaseResult> UpdateWorkflow(UpdateWorkflowDto updateWorkflowDto);
        Task<BaseResult> RemoveWorkflow(RemoveWorkflowDto removeWorkflowDto);

        // Step
        Task<BaseResult> CreateStep(string workflowId, CreateStepDto createStepDto);
        Task<BaseResult> CreateListStep(string workflowId, List<CreateStepDto> createListStepDto);
        Task<BaseResult> UpdateStep(string workflowId, UpdateStepDto updateStepDto);
        Task<BaseResult> UpdateListStep(string workflowId, List<UpdateStepDto> updateListStepDto);
        Task<BaseResult> RemoveStep(string workflowId, string stepId);
        Task<BaseResult> RemoveListStep(string workflowId, List<string> listStepId);
        Task<BaseResult> RemoveAllStep(string workflowId);
    }
}