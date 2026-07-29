using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Order;
using Services.Dtos.Workflow;
using Services.Implements;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/workflows")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost]
        public async Task<ActionResult<BaseResult>> Post([FromBody] CreateWorkflowDto createWorkflowDto)
        {
            var response = await _workflowService.CreateWorkflow(createWorkflowDto);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        //[HttpGet]
        //public async Task<ActionResult<BaseResult>> Get([FromQuery] string workflowId)
        //{
        //    var response = await _workflowService.GetWorkflow(workflowId);
        //    return StatusCode(StatusCodes.Status200OK, response);
        //}

        [HttpPut]
        public async Task<ActionResult<BaseResult>> Update([FromBody] UpdateWorkflowDto updateWorkflow)
        {
            var response = await _workflowService.UpdateWorkflow(updateWorkflow);
            return StatusCode(StatusCodes.Status202Accepted, response);
        }

        [HttpDelete]
        public async Task<ActionResult<BaseResult>> Delete([FromBody] RemoveWorkflowDto removeWorkflowDto)
        {
            var response = await _workflowService.RemoveWorkflow(removeWorkflowDto);
            return StatusCode(StatusCodes.Status202Accepted, response);
        }
    }
}
