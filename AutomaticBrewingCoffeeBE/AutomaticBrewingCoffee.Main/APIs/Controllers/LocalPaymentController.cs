using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Services.CapRabbitMQ.Messages.Payment;
using Services.CapRabbitMQ.Topics;
using Services.MPOS.Data;

namespace AutomaticBrewingCoffee.API.Controllers;

[ApiController]
[Route($"{ApiEndpointsConstant.API_ENDPOINT}/local-payments")]
public sealed class LocalPaymentController : ControllerBase
{
    private readonly IHostEnvironment _environment;
    private readonly ICapPublisher _capPublisher;
    private readonly AutoBrewingBeContext _context;

    public LocalPaymentController(
        IHostEnvironment environment,
        ICapPublisher capPublisher,
        AutoBrewingBeContext context)
    {
        _environment = environment;
        _capPublisher = capPublisher;
        _context = context;
    }

    [HttpPost("{orderId}/success")]
    [ApiKeyAuth]
    public async Task<IActionResult> MarkSuccess(string orderId)
    {
        if (!_environment.IsEnvironment("Local"))
            return NotFound();

        var order = await _context.Orders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrderId == orderId);

        if (order is null)
            return NotFound();

        if (!string.Equals(order.Status, nameof(EOrderStatus.Pending), StringComparison.Ordinal))
        {
            return Accepted(new
            {
                isSuccess = true,
                orderId,
                paymentMode = "sandbox-success-button",
                alreadyHandled = true,
                orderStatus = order.Status
            });
        }

        await _capPublisher.PublishAsync(
            PaymentCapTopic.PaymentMPOSCallback,
            new PaymentMPOSCallbackMessage
            {
                OrderId = orderId,
                TranStatusEnum = nameof(MPOSTransStatus.Approved),
                TransStatus = (long)MPOSTransStatus.Approved,
                TransDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TransAmount = 0,
                ServiceName = "local-sandbox",
                TransCode = string.Empty,
                IssuerCode = string.Empty,
                Muid = string.Empty,
                PosId = string.Empty
            });

        return Accepted(new
        {
            isSuccess = true,
            orderId,
            paymentMode = "sandbox-success-button"
        });
    }
}
