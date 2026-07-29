using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Services.Utils;
using Microsoft.Extensions.Logging;
using Services.Dtos.Sync;

namespace Services.BackgroundJobs;

public class KioskSynchronizedDataJob(IUnitOfWork unitOfWork, ILogger<KioskSynchronizedDataJob> logger)
{
    public async Task SyncKioskMenuAutomatically()
    {
        logger.LogInformation("KioskSynchronizedDataJob started at {Time}", DateTime.UtcNow);
        await SyncKioskMenus();
        logger.LogInformation("KioskSynchronizedDataJob finished at {Time}", DateTime.UtcNow);
    }

    private async Task SyncKioskMenus()
    {
        var kiosks = await unitOfWork.GetRepository<SyncTask>().GetListAsync();
        var kioskIds = kiosks.Select(x => x.KioskId).Distinct().ToList();

        logger.LogInformation("Found {Count} kiosk(s) with sync tasks", kioskIds.Count);

        foreach (var kioskId in kioskIds)
        {
            logger.LogInformation("Processing sync for kiosk {KioskId}", kioskId);
            await SyncKioskMenu(kioskId);
        }
    }

    private async Task SyncKioskMenu(string kioskId)
    {
        var syncTasks = await unitOfWork.GetRepository<SyncTask>().GetListAsync(
            predicate: x => x.KioskId == kioskId && x.IsSynced == false
        );

        if (!syncTasks.Any())
        {
            logger.LogInformation("No pending sync tasks for kiosk {KioskId}", kioskId);
            return;
        }

        var syncEvents = new List<SyncEvent>();

        foreach (var syncTask in syncTasks)
        {
            var syncEvent = await unitOfWork.GetRepository<SyncEvent>()
                .SingleOrDefaultAsync(predicate: x => x.SyncEventId == syncTask.SyncEventId);

            if (syncEvent != null)
            {
                syncEvents.Add(syncEvent);
            }
            else
            {
                logger.LogWarning("SyncEvent not found for SyncTask {SyncTaskId}", syncTask.SyncTaskId);
            }
        }

        var syncActions = new SyncActions();

        foreach (var syncEvent in syncEvents)
        {
            var entity = await GetEntityAsync(syncEvent);

            if (entity != null)
            {
                var syncAction = syncActions.GetSyncAction<dynamic>(syncEvent.EntityType);

                switch (syncEvent.SyncEventType)
                {
                    case nameof(ESyncEventType.Create):
                        syncAction.Create.Add(entity);
                        break;
                    case nameof(ESyncEventType.Update):
                        syncAction.Update.Add(entity);
                        break;
                    case nameof(ESyncEventType.Delete):
                        syncAction.Delete.Add(entity);
                        break;
                    default:
                        logger.LogWarning("Unknown SyncEventType {SyncEventType} in SyncEvent {SyncEventId}",
                            syncEvent.SyncEventType, syncEvent.SyncEventId);
                        break;
                }
            }
            else
            {
                logger.LogWarning("Entity not found for SyncEvent {SyncEventId} of type {EntityType}",
                    syncEvent.SyncEventId, syncEvent.EntityType);
            }
        }

        var kioskDataSyncDto = new SynchronizedKioskDataDto()
        {
            SyncActions = syncActions
        };

        var webhook = await unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId && x.WebhookType == EWebhookType.SynchronizedData.ToString()
        );

        if (webhook is null)
        {
            logger.LogWarning("No webhook found for kiosk {KioskId} (type: SynchronizedData)", kioskId);
            return;
        }

        var result = await ApiUtil.PostAsync(webhook.WebhookUrl, kioskDataSyncDto);

        if (result.IsSuccessStatusCode)
        {
            foreach (var syncTask in syncTasks)
            {
                syncTask.Sync();
            }

            unitOfWork.GetRepository<SyncTask>().UpdateRange(syncTasks);
            await unitOfWork.CommitAsync();

            logger.LogInformation("Successfully synced {Count} tasks for kiosk {KioskId}", syncTasks.Count, kioskId);
        }
        else
        {
            logger.LogError("Failed to sync kiosk {KioskId}. Webhook response: {StatusCode}", kioskId,
                result.StatusCode);
        }
    }

    private async Task<object?> GetEntityAsync(SyncEvent syncEvent)
    {
        return syncEvent.EntityType switch
        {
            nameof(Product) => await unitOfWork.GetRepository<Product>()
                .SingleOrDefaultAsync(predicate: x => x.ProductId == syncEvent.EntityId),
            nameof(Workflow) => await unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(predicate: x => x.WorkflowId == syncEvent.EntityId),
            nameof(Step) => await unitOfWork.GetRepository<Step>()
                .SingleOrDefaultAsync(predicate: x => x.StepId == syncEvent.EntityId),
            nameof(Menu) => await unitOfWork.GetRepository<Menu>()
                .SingleOrDefaultAsync(predicate: x => x.MenuId == syncEvent.EntityId),
            nameof(MenuProductMapping) => await unitOfWork.GetRepository<MenuProductMapping>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.MenuId == syncEvent.EntityId && x.ProductId == syncEvent.SecondEntityId),
            nameof(Kiosk) => await unitOfWork.GetRepository<Kiosk>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.KioskId == syncEvent.EntityId),
            nameof(KioskDeviceMapping) => await unitOfWork.GetRepository<KioskDeviceMapping>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.KioskDeviceMappingId == syncEvent.EntityId),
            _ => null
        };
    }
}