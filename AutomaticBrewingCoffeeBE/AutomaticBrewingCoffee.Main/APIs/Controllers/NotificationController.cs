using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Notification;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers;

[Route($"{ApiEndpointsConstant.API_ENDPOINT}/notifications")]
[ApiController]
[TrimStrings]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/notifications
    [HttpGet]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Get list of notifications",
        Description = "Retrieve a paginated list of notifications with optional filters such as receiverId and type."
    )]
    public async Task<ActionResult<BaseResult<NotificationQueryDto, Paginate<NotificationDto>>>> Get(
        [FromQuery] NotificationQueryDto notificationQueryDto)
    {
        var response = await _notificationService.GetNotifications(notificationQueryDto);
        return StatusCode(response.StatusCode, response);
    }

    // GET: api/notifications/{notificationId}
    [HttpGet("{notificationId}")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Get notification detail",
        Description = "Retrieve detailed information about a specific notification by its ID."
    )]
    public async Task<ActionResult<BaseResult<string, NotificationDto>>> Get(string notificationId)
    {
        var response = await _notificationService.GetNotification(notificationId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("read-notification")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Mark a notification as read",
        Description =
            "Mark a specific notification as read by its ID. This updates the read status without modifying other data."
    )]
    public async Task<ActionResult<BaseResult<string, NotificationDto>>> ReadNotification(
        ReadNotificationDto readNotificationDto)
    {
        var response = await _notificationService.ReadNotification(readNotificationDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("read-notifications")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Mark multiple notifications as read",
        Description =
            "Mark a list of notifications as read using their IDs. Useful for batch updates of notification read status."
    )]
    public async Task<ActionResult<BaseResult<string, NotificationDto>>> ReadNotifications(
        ReadNotificationsDto readNotificationsDto)
    {
        var response = await _notificationService.ReadNotifications(readNotificationsDto);
        return StatusCode(response.StatusCode, response);
    }
}