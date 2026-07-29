using AutomaticBrewingCoffee.API.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Firebase;
using Services.Local;
using Services.Utils;

namespace Services.Tests.Local;

public class LocalStartupCompositionTests
{
    [Fact]
    public void IsLocalMode_AcceptsLocalEnvironmentOrExplicitFlag()
    {
        var localEnvironment = new ConfigurationBuilder().Build();
        var explicitFlag = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LOCAL_MODE"] = "true"
            })
            .Build();

        Assert.True(LocalRuntimeDependency.IsLocalMode("Local", localEnvironment));
        Assert.True(LocalRuntimeDependency.IsLocalMode("Production", explicitFlag));
        Assert.False(LocalRuntimeDependency.IsLocalMode("Production", localEnvironment));
    }

    [Fact]
    public void BackgroundJobs_PreserveNonLocalDefaultAndRequireLocalOptIn()
    {
        var defaults = new ConfigurationBuilder().Build();
        var explicitLocalOptIn = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundJobs:Enabled"] = "true"
            })
            .Build();

        Assert.True(LocalRuntimeDependency.AreBackgroundJobsEnabled(false, defaults));
        Assert.False(LocalRuntimeDependency.AreBackgroundJobsEnabled(true, defaults));
        Assert.True(LocalRuntimeDependency.AreBackgroundJobsEnabled(true, explicitLocalOptIn));
    }

    [Fact]
    public void AddLocalRuntime_ReplacesFirebaseWithDisabledImplementation()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        services.AddLocalRuntime(configuration);

        var registration = Assert.Single(
            services.Where(x => x.ServiceType == typeof(IFirebaseAuthService)));

        Assert.Equal(typeof(DisabledFirebaseAuthService), registration.ImplementationType);
    }

    [Fact]
    public async Task DisabledFirebaseAuthService_DoesNotAttemptNetworkAuthentication()
    {
        var service = new DisabledFirebaseAuthService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoginByEmailPassword("local@cupx.test", "local-password"));

        Assert.Contains("disabled in local mode", exception.Message);
    }

    [Fact]
    public void ApiKeyEncryption_RequiresAnExplicitValidKey()
    {
        const string encryptionKey = "0123456789ABCDEF";
        const string plainText = "local-kiosk-api-key";

        var encrypted = ApiKeyUtil.Encrypt(plainText, encryptionKey);

        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, ApiKeyUtil.Decrypt(encrypted, encryptionKey));
        Assert.Throws<ArgumentException>(() => ApiKeyUtil.Encrypt(plainText, "too-short"));
    }

    [Fact]
    public void OrderHub_DoesNotWriteApiKeyValueToLogs()
    {
        var repoRoot = FindRepoRoot();
        var orderHubPath = Path.Combine(
            repoRoot,
            "AutomaticBrewingCoffeeBE",
            "AutomaticBrewingCoffee.Main",
            "Services",
            "SignalR",
            "OrderHub.cs");
        var source = File.ReadAllText(orderHubPath);

        Assert.DoesNotContain("ApiKey = {apiKey}", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey = $", source, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "compose.local.yml")))
        {
            current = current.Parent;
        }

        return current?.FullName
            ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
