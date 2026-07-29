using AutomaticBrewingCoffee.API.Health;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Services.Firebase;
using Services.Local;
using Services.Supabase;

namespace AutomaticBrewingCoffee.API.Extensions;

public static class LocalRuntimeDependency
{
    public static bool IsLocalMode(string environmentName, IConfiguration configuration)
    {
        return string.Equals(environmentName, "Local", StringComparison.OrdinalIgnoreCase)
            || configuration.GetValue<bool>("LOCAL_MODE");
    }

    public static bool AreBackgroundJobsEnabled(bool localMode, IConfiguration configuration)
    {
        return localMode
            ? configuration.GetValue<bool>("BackgroundJobs:Enabled")
            : configuration.GetValue("BackgroundJobs:Enabled", true);
    }

    public static IServiceCollection AddLocalRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RemoveAll<IFirebaseAuthService>();
        services.AddScoped<IFirebaseAuthService, DisabledFirebaseAuthService>();

        services.RemoveAll<ISupabaseStorageService>();
        services.AddScoped<ISupabaseStorageService, DisabledSupabaseStorageService>();

        services.Configure<LocalSeedOptions>(configuration.GetSection("LocalSeed"));
        services.AddScoped<LocalDevelopmentSeeder>();
        services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>("sqlserver")
            .AddCheck<RedisHealthCheck>("redis")
            .AddCheck<RabbitMqHealthCheck>("rabbitmq");
        return services;
    }
}
