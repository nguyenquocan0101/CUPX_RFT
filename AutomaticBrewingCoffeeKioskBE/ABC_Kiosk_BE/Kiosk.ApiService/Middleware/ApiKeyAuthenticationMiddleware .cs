
using AutomaticBrewingCoffee.API.Constants;

namespace Kiosk.ApiService.Middleware
{
    public class ApiKeyAuthenticationMiddleware(IApiKeyValidatorService apiKeyValidator) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ConstantValue.ApiKeyHeaderName, out var extractedApiKey) || !apiKeyValidator.IsValid(extractedApiKey))
            {
                throw new UnauthorizedAccessException("Api Key is missing");
            }
            await next(context);
        }
    }


    public interface IApiKeyValidatorService
    {
        bool IsValid(string apiKey);
    }

    public class ApiKeyValidatorService : IApiKeyValidatorService
    {
        private readonly IConfiguration _configuration;

        public ApiKeyValidatorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValid(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;
            var validKey = _configuration[ConstantValue.ApiKeyName]; 
            return apiKey == validKey;
        }
    }

}
