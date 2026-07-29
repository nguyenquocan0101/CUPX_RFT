namespace Kiosk.ApiService.Extensions;

public static class KioskRuntimeSettings
{
    public static bool IsLocalMode(IConfiguration configuration)
    {
        return string.Equals(configuration["LOCAL_MODE"], "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Local", StringComparison.OrdinalIgnoreCase);
    }

    public static bool AreWorkflowWorkersEnabled(IConfiguration configuration)
    {
        var configuredValue = configuration["WORKFLOW_WORKERS_ENABLED"];
        if (bool.TryParse(configuredValue, out var enabled))
        {
            return enabled;
        }

        return !IsLocalMode(configuration);
    }
}
