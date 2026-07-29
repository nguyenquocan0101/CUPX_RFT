using Services.Local;
using Services.Utils;

namespace Services.Tests.Local;

public class LocalSeedTests
{
    [Fact]
    public void CreateSeedData_UsesStableIdsAndUsableCredentials()
    {
        var options = new LocalSeedOptions
        {
            Enabled = true,
            AdminEmail = "admin@cupx.local",
            AdminPassword = "local-admin-password",
            KioskApiKey = "local-kiosk-api-key",
            KioskBaseUrl = "http://localhost:5160"
        };
        const string encryptionKey = "0123456789ABCDEF";

        var first = LocalSeedData.Create(options, encryptionKey);
        var second = LocalSeedData.Create(options, encryptionKey);

        Assert.Equal(first.Account.AccountId, second.Account.AccountId);
        Assert.Equal(first.Organization.OrganizationId, second.Organization.OrganizationId);
        Assert.Equal(first.Kiosk.KioskId, second.Kiosk.KioskId);
        Assert.Equal(first.Product.ProductId, second.Product.ProductId);
        Assert.Equal(
            first.KioskVersionProductMapping.ProductId,
            second.KioskVersionProductMapping.ProductId);
        Assert.Equal(
            first.KioskVersionDeviceModelMapping.DeviceModelId,
            second.KioskVersionDeviceModelMapping.DeviceModelId);
        Assert.Equal(first.Webhooks.Select(x => x.WebhookId), second.Webhooks.Select(x => x.WebhookId));
        Assert.True(Hasher.Verify(options.AdminPassword, first.Account.Password));
        Assert.Equal(options.KioskApiKey, ApiKeyUtil.Decrypt(first.Kiosk.ApiKey!, encryptionKey));
    }
}
