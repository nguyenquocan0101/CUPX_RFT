using Services.Storage;
using Services.Supabase;
using Services.Local;

namespace AutomaticBrewingCoffee.API.Extensions;

public static class LocalProviderDependency
{
    public static IServiceCollection AddLocalProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var minioOptions = configuration.GetSection("MINIO").Get<MinioOptions>() ?? new MinioOptions();
        services.AddSingleton(minioOptions);
        services.AddSingleton<MinioObjectStorageService>(sp =>
            new MinioObjectStorageService(sp.GetRequiredService<MinioOptions>()));
        services.AddSingleton<ISupabaseStorageService>(sp =>
            sp.GetRequiredService<MinioObjectStorageService>());
        services.AddSingleton<IObjectStorageService>(sp =>
            sp.GetRequiredService<MinioObjectStorageService>());

        var webhookOptions = configuration.GetSection("WEBHOOK").Get<LocalWebhookOptions>()
            ?? new LocalWebhookOptions();
        webhookOptions.ApiKey = configuration["LocalSeed:KioskApiKey"] ?? string.Empty;
        services.AddSingleton(webhookOptions);
        services.AddScoped<ILocalWebhookPersistence, SqlLocalWebhookPersistence>();
        services.AddScoped<LocalWebhookTrigger>(sp => new LocalWebhookTrigger(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
            sp.GetRequiredService<LocalWebhookOptions>(),
            sp.GetRequiredService<ILocalWebhookPersistence>()));

        return services;
    }
}
