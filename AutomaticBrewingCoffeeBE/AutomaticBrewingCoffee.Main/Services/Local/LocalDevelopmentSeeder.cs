using AutomaticBrewingCoffee.Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Services.Local;

public sealed class LocalDevelopmentSeeder(
    AutoBrewingBeContext context,
    IOptions<LocalSeedOptions> options)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        var encryptionKey = Environment.GetEnvironmentVariable("CUPX_API_KEY_ENCRYPTION_KEY")
            ?? throw new InvalidOperationException(
                "CUPX_API_KEY_ENCRYPTION_KEY must be set before local data is seeded.");
        var data = LocalSeedData.Create(options.Value, encryptionKey);

        if (!await context.Organizations.IgnoreQueryFilters()
                .AnyAsync(x => x.OrganizationId == data.Organization.OrganizationId, cancellationToken))
        {
            context.Organizations.Add(data.Organization);
        }

        if (!await context.Accounts.IgnoreQueryFilters()
                .AnyAsync(x => x.AccountId == data.Account.AccountId, cancellationToken))
        {
            context.Accounts.Add(data.Account);
        }

        if (!await context.Stores.IgnoreQueryFilters()
                .AnyAsync(x => x.StoreId == data.Store.StoreId, cancellationToken))
        {
            context.Stores.Add(data.Store);
        }

        if (!await context.KioskTypes.IgnoreQueryFilters()
                .AnyAsync(x => x.KioskTypeId == data.KioskType.KioskTypeId, cancellationToken))
        {
            context.KioskTypes.Add(data.KioskType);
        }

        if (!await context.KioskVersions.IgnoreQueryFilters()
                .AnyAsync(x => x.KioskVersionId == data.KioskVersion.KioskVersionId, cancellationToken))
        {
            context.KioskVersions.Add(data.KioskVersion);
        }

        if (!await context.Menus.IgnoreQueryFilters()
                .AnyAsync(x => x.MenuId == data.Menu.MenuId, cancellationToken))
        {
            context.Menus.Add(data.Menu);
        }

        if (!await context.Set<AutomaticBrewingCoffee.Domain.Models.ProductCategory>().IgnoreQueryFilters()
                .AnyAsync(x => x.ProductCategoryId == data.ProductCategory.ProductCategoryId, cancellationToken))
        {
            context.Add(data.ProductCategory);
        }

        if (!await context.Products.IgnoreQueryFilters()
                .AnyAsync(x => x.ProductId == data.Product.ProductId, cancellationToken))
        {
            context.Products.Add(data.Product);
        }

        if (!await context.MenuProductMappings.IgnoreQueryFilters()
                .AnyAsync(
                    x => x.MenuId == data.MenuProductMapping.MenuId
                        && x.ProductId == data.MenuProductMapping.ProductId,
                    cancellationToken))
        {
            context.MenuProductMappings.Add(data.MenuProductMapping);
        }

        if (!await context.DeviceTypes.IgnoreQueryFilters()
                .AnyAsync(x => x.DeviceTypeId == data.DeviceType.DeviceTypeId, cancellationToken))
        {
            context.DeviceTypes.Add(data.DeviceType);
        }

        if (!await context.DeviceModels.IgnoreQueryFilters()
                .AnyAsync(x => x.DeviceModelId == data.DeviceModel.DeviceModelId, cancellationToken))
        {
            context.DeviceModels.Add(data.DeviceModel);
        }

        if (!await context.Devices.IgnoreQueryFilters()
                .AnyAsync(x => x.DeviceId == data.Device.DeviceId, cancellationToken))
        {
            context.Devices.Add(data.Device);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.KioskVersionProductMappings.IgnoreQueryFilters()
                .AnyAsync(
                    x => x.KioskVersionId == data.KioskVersionProductMapping.KioskVersionId
                        && x.ProductId == data.KioskVersionProductMapping.ProductId,
                    cancellationToken))
        {
            context.KioskVersionProductMappings.Add(data.KioskVersionProductMapping);
        }

        if (!await context.KioskVersionDeviceModelMappings.IgnoreQueryFilters()
                .AnyAsync(
                    x => x.KioskVersionId == data.KioskVersionDeviceModelMapping.KioskVersionId
                        && x.DeviceModelId == data.KioskVersionDeviceModelMapping.DeviceModelId,
                    cancellationToken))
        {
            context.KioskVersionDeviceModelMappings.Add(data.KioskVersionDeviceModelMapping);
        }

        if (!await context.Workflows.IgnoreQueryFilters()
                .AnyAsync(x => x.WorkflowId == data.Workflow.WorkflowId, cancellationToken))
        {
            context.Workflows.Add(data.Workflow);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.Steps.IgnoreQueryFilters()
                .AnyAsync(x => x.StepId == data.Step.StepId, cancellationToken))
        {
            context.Steps.Add(data.Step);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.Kiosks.IgnoreQueryFilters()
                .AnyAsync(x => x.KioskId == data.Kiosk.KioskId, cancellationToken))
        {
            context.Kiosks.Add(data.Kiosk);
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.KioskDeviceMappings.IgnoreQueryFilters()
                .AnyAsync(
                    x => x.KioskDeviceMappingId == data.KioskDeviceMapping.KioskDeviceMappingId,
                    cancellationToken))
        {
            context.KioskDeviceMappings.Add(data.KioskDeviceMapping);
        }

        foreach (var webhook in data.Webhooks)
        {
            if (!await context.Webhooks.IgnoreQueryFilters()
                    .AnyAsync(x => x.WebhookId == webhook.WebhookId, cancellationToken))
            {
                context.Webhooks.Add(webhook);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
