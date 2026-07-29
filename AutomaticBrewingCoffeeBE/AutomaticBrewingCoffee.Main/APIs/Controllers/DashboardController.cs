using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Dashboard.Summary.Account;
using Services.Dtos.Dashboard.Summary.Kiosk;
using Services.Dtos.Dashboard.Summary.Order;
using Services.Dtos.Dashboard.Summary.Organization;
using Services.Dtos.Dashboard.Summary.Store;
using Services.Dtos.Dashboard.Total.Revenue;
using Services.Dtos.Dashboard.Traffic.HourlyPeak;
using Services.Dtos.Dashboard.Traffic.Order;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers;

[Route($"{ApiEndpointsConstant.API_ENDPOINT}/dashboard")]
[ApiController]
[TrimStrings]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get order summary statistics within a date range
    /// </summary>
    [HttpGet("order-summary")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get order summary",
        Description =
            "Get total, pending, preparing, completed, cancelled, and failed orders within the given date range.")]
    public async Task<ActionResult<BaseResult<OrderSummaryQueryDto, OrderSummaryDto>>> GetOrderSummary(
        [FromQuery] OrderSummaryQueryDto query)
    {
        var response = await _dashboardService.GetOrderSummary(query);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get kiosk summary statistics within a date range
    /// </summary>
    [HttpGet("kiosk-summary")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get kiosk summary",
        Description =
            "Get total active and inactive kiosk within the given date range.")]
    public async Task<ActionResult<BaseResult<KioskSummaryQueryDto, KioskSummaryDto>>> GetKioskSummary(
        [FromQuery] KioskSummaryQueryDto query)
    {
        var response = await _dashboardService.GetKioskSummary(query);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get organization summary statistics within a date range
    /// </summary>
    [HttpGet("account-summary")]
    [Authorizes(nameof(ERoleName.Admin))]
    [SwaggerOperation(Summary = "Get account summary",
        Description =
            "Get total active and inactive account within the given date range.")]
    public async Task<ActionResult<BaseResult<AccountSummaryQueryDto, AccountSummaryDto>>> GetAccountSummary(
        [FromQuery] AccountSummaryQueryDto query)
    {
        var response = await _dashboardService.GetAccountSummary(query);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get organization summary statistics within a date range
    /// </summary>
    [HttpGet("organization-summary")]
    [Authorizes(nameof(ERoleName.Admin))]
    [SwaggerOperation(Summary = "Get organization summary",
        Description =
            "Get total active and inactive organization within the given date range.")]
    public async Task<ActionResult<BaseResult<KioskSummaryQueryDto, KioskSummaryDto>>> GetOrganizationSummary(
        [FromQuery] OrganizationSummaryQueryDto query)
    {
        var response = await _dashboardService.GetOrganizationSummary(query);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get store summary statistics within a date range
    /// </summary>
    [HttpGet("store-summary")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get store summary",
        Description =
            "Get total active and inactive store within the given date range.")]
    public async Task<ActionResult<BaseResult<StoreSummaryQueryDto, StoreSummaryDto>>> GetStoreSummary(
        [FromQuery] StoreSummaryQueryDto query)
    {
        var response = await _dashboardService.GetStoreSummary(query);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get kiosk summary statistics within a date range
    /// </summary>
    [HttpGet("total-revenue")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get total revenue",
        Description =
            "Get total revenue within the given date range.")]
    public async Task<ActionResult<BaseResult<TotalRevenueQueryDto, TotalRevenueDto>>> GetTotalRevenue(
        [FromQuery] TotalRevenueQueryDto query)
    {
        var response = await _dashboardService.GetTotalRevenue(query);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("order-traffic")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get order traffic",
        Description =
            "Get order traffic within the given date range.")]
    public async Task<ActionResult<BaseResult<OrderTrafficQueryDto, OrderTrafficDto>>> GetOrderTraffic(
        [FromQuery] OrderTrafficQueryDto query)
    {
        var response = await _dashboardService.GetOrderTraffic(query);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("hourly-peak")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(Summary = "Get hourly peak",
        Description =
            "Get hourly peak within the given date range.")]
    public async Task<IActionResult> GetHourlyPeak([FromQuery] HourlyPeakQueryDto query)
    {
        var response = await _dashboardService.GetHourlyPeak(query);
        return StatusCode(response.StatusCode, response);
    }
}