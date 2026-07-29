using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Utils;

namespace Services.Local;

public sealed class LocalSeedData
{
    private static readonly DateTime CreatedDate =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public required Account Account { get; init; }
    public required Organization Organization { get; init; }
    public required Store Store { get; init; }
    public required KioskType KioskType { get; init; }
    public required KioskVersion KioskVersion { get; init; }
    public required Menu Menu { get; init; }
    public required ProductCategory ProductCategory { get; init; }
    public required Product Product { get; init; }
    public required Product ProductMaking { get; init; }
    public required MenuProductMapping MenuProductMapping { get; init; }
    public required KioskVersionProductMapping KioskVersionProductMapping { get; init; }
    public required KioskVersionProductMapping KioskVersionProductMappingMaking { get; init; }
    public required DeviceType DeviceType { get; init; }
    public required DeviceModel DeviceModel { get; init; }
    public required KioskVersionDeviceModelMapping KioskVersionDeviceModelMapping { get; init; }
    public required Device Device { get; init; }
    public required Kiosk Kiosk { get; init; }
    public required KioskDeviceMapping KioskDeviceMapping { get; init; }
    public required Workflow Workflow { get; init; }
    public required Step Step { get; init; }
    public required Workflow WorkflowMaking { get; init; }
    public required Step StepMaking { get; init; }
    public required IReadOnlyList<Webhook> Webhooks { get; init; }

    public static LocalSeedData Create(LocalSeedOptions options, string encryptionKey)
    {
        Validate(options);

        const string organizationId = "local-org";
        const string storeId = "local-store";
        const string kioskTypeId = "local-kiosk-type";
        const string kioskVersionId = "local-kiosk-v1";
        const string menuId = "local-menu";
        const string categoryId = "local-category";
        const string productId = "local-product";
        const string productMakingId = "local-product-making";
        const string deviceTypeId = "local-device-type";
        const string deviceModelId = "local-device-model";
        const string deviceId = "local-device";
        const string kioskId = "local-kiosk";
        const string workflowId = "local-workflow";

        var baseUrl = options.KioskBaseUrl.TrimEnd('/');

        return new LocalSeedData
        {
            Organization = new Organization
            {
                OrganizationId = organizationId,
                OrganizationCode = "LOCAL",
                Name = "CUPX Local",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            Account = new Account
            {
                AccountId = "local-admin",
                OrganizationId = organizationId,
                Email = options.AdminEmail,
                Password = Hasher.Hash(options.AdminPassword),
                FullName = "Local Administrator",
                RoleName = "Admin",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            Store = new Store
            {
                StoreId = storeId,
                OrganizationId = organizationId,
                Name = "CUPX Local Store",
                LocationAddress = "Local machine",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            KioskType = new KioskType
            {
                KioskTypeId = kioskTypeId,
                Name = "Local Kiosk",
                Description = "Local development kiosk",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            KioskVersion = new KioskVersion
            {
                KioskVersionId = kioskVersionId,
                KioskTypeId = kioskTypeId,
                VersionTitle = "Local v1",
                VersionNumber = "1.0.0",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            Menu = new Menu
            {
                MenuId = menuId,
                OrganizationId = organizationId,
                Name = "Local Menu",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            ProductCategory = new ProductCategory
            {
                ProductCategoryId = categoryId,
                Name = "Local Drinks",
                Status = EBaseStatus.Active.ToString(),
                DisplayOrder = 1,
                CreatedDate = CreatedDate
            },
            Product = new Product
            {
                ProductId = productId,
                ProductCategoryId = categoryId,
                Name = "Local Coffee",
                Status = EBaseStatus.Active.ToString(),
                Type = "Drink",
                Price = 20000m,
                CreatedDate = CreatedDate
            },
            ProductMaking = new Product
            {
                ProductId = productMakingId,
                ParentId = productId,
                ProductCategoryId = categoryId,
                Name = "Local Coffee Making",
                Status = EBaseStatus.Active.ToString(),
                Type = "Making",
                Price = 0m,
                CreatedDate = CreatedDate
            },
            MenuProductMapping = new MenuProductMapping
            {
                MenuId = menuId,
                ProductId = productId,
                DisplayOrder = 1,
                SellingPrice = 20000m,
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            KioskVersionProductMapping = new KioskVersionProductMapping
            {
                KioskVersionId = kioskVersionId,
                ProductId = productId,
                CreatedDate = CreatedDate
            },
            KioskVersionProductMappingMaking = new KioskVersionProductMapping
            {
                KioskVersionId = kioskVersionId,
                ProductId = productMakingId,
                CreatedDate = CreatedDate
            },
            DeviceType = new DeviceType
            {
                DeviceTypeId = deviceTypeId,
                Name = "Local Brewer",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            DeviceModel = new DeviceModel
            {
                DeviceModelId = deviceModelId,
                DeviceTypeId = deviceTypeId,
                ModelName = "Local Model",
                Manufacturer = "CUPX",
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            KioskVersionDeviceModelMapping = new KioskVersionDeviceModelMapping
            {
                KioskVersionId = kioskVersionId,
                DeviceModelId = deviceModelId,
                Quantity = 1,
                CreatedDate = CreatedDate
            },
            Device = new Device
            {
                DeviceId = deviceId,
                DeviceModelId = deviceModelId,
                SerialNumber = "LOCAL-001",
                Name = "Local Brewer",
                Description = "Local development device",
                Status = EDeviceStatus.Working.ToString(),
                IsOnHub = false,
                CreatedDate = CreatedDate
            },
            Kiosk = new Kiosk
            {
                KioskId = kioskId,
                StoreId = storeId,
                KioskVersionId = kioskVersionId,
                MenuId = menuId,
                ApiKey = ApiKeyUtil.Encrypt(options.KioskApiKey, encryptionKey),
                Hostname = "localhost",
                OriginServer = baseUrl,
                Location = "Local machine",
                Status = EKioskDeviceStatus.Online.ToString(),
                InstalledDate = CreatedDate,
                CreatedDate = CreatedDate
            },
            KioskDeviceMapping = new KioskDeviceMapping
            {
                KioskDeviceMappingId = "local-kiosk-device",
                KioskId = kioskId,
                DeviceId = deviceId,
                Side = ESide.Left.ToString(),
                Status = EBaseStatus.Active.ToString(),
                CreatedDate = CreatedDate
            },
            Workflow = new Workflow
            {
                WorkflowId = workflowId,
                ProductId = productId,
                KioskVersionId = kioskVersionId,
                Name = "Local Coffee Workflow",
                Type = EWorkflowType.Activity.ToString(),
                CreatedDate = CreatedDate
            },
            Step = new Step
            {
                StepId = "local-step",
                WorkflowId = workflowId,
                DeviceModelId = deviceModelId,
                Name = "Complete local order",
                Type = EStepType.CompleteOrderCommand.ToString(),
                Sequence = 1,
                CreatedDate = CreatedDate
            },
            WorkflowMaking = new Workflow
            {
                WorkflowId = "local-workflow-making",
                ProductId = productMakingId,
                KioskVersionId = kioskVersionId,
                Name = "Local Coffee Making Workflow",
                Description = "Deterministic local workflow fixture for the product variant",
                Type = EWorkflowType.Activity.ToString(),
                CreatedDate = CreatedDate
            },
            StepMaking = new Step
            {
                StepId = "local-step-making",
                WorkflowId = "local-workflow-making",
                DeviceModelId = deviceModelId,
                Name = "Simulator dispense",
                Type = "dispense",
                Sequence = 1,
                Parameters = "{}",
                CreatedDate = CreatedDate
            },
            Webhooks =
            [
                new Webhook
                {
                    WebhookId = "local-webhook-health",
                    KioskId = kioskId,
                    WebhookType = EWebhookType.HealthCheck.ToString(),
                    WebhookUrl = $"{baseUrl}/api/v1/ping",
                    CreatedDate = CreatedDate
                },
                new Webhook
                {
                    WebhookId = "local-webhook-execute",
                    KioskId = kioskId,
                    WebhookType = EWebhookType.ExecuteProduct.ToString(),
                    WebhookUrl = $"{baseUrl}/api/v1/execute",
                    CreatedDate = CreatedDate
                }
            ]
        };
    }

    private static void Validate(LocalSeedOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AdminEmail)
            || string.IsNullOrWhiteSpace(options.AdminPassword)
            || string.IsNullOrWhiteSpace(options.KioskApiKey)
            || string.IsNullOrWhiteSpace(options.KioskBaseUrl))
        {
            throw new InvalidOperationException(
                "Local seed credentials and kiosk base URL must be configured.");
        }
    }
}
