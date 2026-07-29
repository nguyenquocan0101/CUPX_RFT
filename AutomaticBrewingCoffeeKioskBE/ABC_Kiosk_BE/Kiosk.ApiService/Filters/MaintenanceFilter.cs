using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.Base;
using Services.Interfaces;

namespace Kiosk.ApiService.Filters
{
    public class MaintenanceFilter : IAsyncActionFilter
    {
        private readonly IRuntimeStateService _state;

        public MaintenanceFilter(IRuntimeStateService state)
        {
            _state = state;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isMaintenance = await _state.IsMaintenanceAsync();
            if (isMaintenance)
            {
                var payload = new BaseResult(
                    StatusCodes.Status503ServiceUnavailable,
                    "System is cleaning. Can not execute other cleaning workflow",
                    false
                );

                context.Result = new JsonResult(payload)
                {
                    StatusCode = payload.StatusCode
                };
                return;
            }

            await next();
        }
    }
}
