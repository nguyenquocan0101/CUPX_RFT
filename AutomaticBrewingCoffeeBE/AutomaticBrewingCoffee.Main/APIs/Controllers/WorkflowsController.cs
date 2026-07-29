using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Step;
using Services.Dtos.Workflow;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/workflows")]
    [ApiController]
    [TrimStrings]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowsController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get all workflows",
            Description = "Returns a list of all workflows"
        )]
        public async Task<ActionResult<BaseResult<WorkflowQueryDto, Paginate<WorkflowDto>>>> Get(
            [FromQuery] WorkflowQueryDto workflowQueryDto)
        {
            var response = await _workflowService.GetWorkflows(workflowQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{workflowId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get workflow by ID",
            Description = "Returns a specific workflow by its ID"
        )]
        public async Task<ActionResult<BaseResult<string, WorkflowDto>>> Get(string workflowId)
        {
            var response = await _workflowService.GetWorkflow(workflowId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new workflow",
            Description = "Creates a new workflow and returns it"
        )]
        public async Task<ActionResult<BaseResult<string, WorkflowDto>>> Post(
            [FromBody] CreateWorkflowDto createWorkflowDto)
        {
            var response = await _workflowService.CreateWorkflow(createWorkflowDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{workflowId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update an existing workflow",
            Description = "Updates an existing workflow by its ID"
        )]
        public async Task<ActionResult<BaseResult<string, WorkflowDto>>> Put(string workflowId,
            [FromBody] UpdateWorkflowDto updateWorkflowDto)
        {
            var response = await _workflowService.UpdateWorkflow(workflowId, updateWorkflowDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{workflowId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a workflow",
            Description = "Deletes a workflow by its ID"
        )]
        public async Task<ActionResult<BaseResult<string, WorkflowDto>>> Delete(string workflowId)
        {
            var response = await _workflowService.RemoveWorkflow(workflowId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("steps")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a step",
            Description = "Create a step"
        )]
        public async Task<ActionResult<BaseResult<CreateStepDto, StepDto>>> CreateStep(
            [FromBody] CreateStepDto createStepDto)
        {
            var response = await _workflowService.CreateStep(createStepDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("steps/{stepId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Remove a step by its id",
            Description = "Remove a step by its id"
        )]
        public async Task<ActionResult<BaseResult<string, StepDto>>> RemoveStep(string stepId)
        {
            var response = await _workflowService.RemoveStep(stepId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("steps/{stepId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Remove a step by its id",
            Description = "Remove a step by its id"
        )]
        public async Task<ActionResult<BaseResult<UpdateStepDto, StepDto>>> UpdateStep(string stepId,
            UpdateStepDto updateStepDto)
        {
            var response = await _workflowService.UpdateStep(stepId, updateStepDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{workflowId}/steps/reorder")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Reorder a step by its id",
            Description = "Reorder a a step by its id"
        )]
        public async Task<ActionResult<BaseResult<UpdateStepDto, StepDto>>> ReorderStep(string workflowId,
            ReorderStepDto reorderStepDto)
        {
            var response = await _workflowService.ReorderStep(workflowId, reorderStepDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}