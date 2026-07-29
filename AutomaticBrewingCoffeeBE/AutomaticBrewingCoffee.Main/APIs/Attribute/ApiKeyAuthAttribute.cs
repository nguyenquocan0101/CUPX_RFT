using System.Security.Claims;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.Utils;

namespace AutomaticBrewingCoffee.API.Attribute;

public sealed class ApiKeyAuthAttribute : ActionFilterAttribute
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check the jwt first
        if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var apiKey = ApiKeyUtil.Encrypt(extractedApiKey[0]!);

        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

        var kiosk = await unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.ApiKey == apiKey
        );

        if (kiosk == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (kiosk.Status != EBaseStatus.Active.ToString())
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (kiosk.IsRevoke)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("kioskId", kiosk.KioskId),
        };
        var identity = new ClaimsIdentity(claims, "ApiKey");
        var principal = new ClaimsPrincipal(identity);
        context.HttpContext.User = principal;

        await next();
    }
}