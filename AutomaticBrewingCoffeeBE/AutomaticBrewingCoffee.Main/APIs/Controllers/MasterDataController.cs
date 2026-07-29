using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Interfaces;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/master-data")]
    [ApiController]
    [TrimStrings]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        [HttpGet("workflow-type")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetWorkflowType()
        {
            return _masterDataService.GetEnumMetadata<EWorkflowType>();
        }

        [HttpGet("webhook-type")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetWebhookType()
        {
            return _masterDataService.GetEnumMetadata<EWebhookType>();
        }

        [HttpGet("product-type")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetProductType()
        {
            return _masterDataService.GetEnumMetadata<EProductType>();
        }

        [HttpGet("kiosk-device-status")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetKioskDeviceStatus()
        {
            return _masterDataService.GetEnumMetadata<EKioskDeviceStatus>();
        }

        [HttpGet("step-type")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetStepType()
        {
            return _masterDataService.GetEnumMetadata<EStepType>();
        }

        [HttpGet("device-status")]
        [Authorizes(nameof(ERoleName.Admin))]
        public ActionResult<BaseResult> GetDeviceStatus()
        {
            return _masterDataService.GetEnumMetadata<EDeviceStatus>();
        }
    }
}