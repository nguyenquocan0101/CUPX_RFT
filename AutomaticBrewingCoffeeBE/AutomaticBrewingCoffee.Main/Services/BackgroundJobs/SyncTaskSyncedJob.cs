using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.BackgroundJobs;

public class SyncTaskSyncedJob(IUnitOfWork unitOfWork, ILogger<SyncTaskSyncedJob> logger)
{
    public async Task SyncTaskSyncedAutomatically()
    {
        await CheckSyncTaskSynced();
    }

    public async Task MarkAllSyncTaskSyncedManually(string kioskId)
    {
        await MarkAllSyncTaskSynced(kioskId);
    }

    private async Task CheckSyncTaskSynced()
    {
        try
        {
            logger.LogInformation("Starting to check and delete synced SyncTasks...");

            var syncTasks = await unitOfWork.GetRepository<SyncTask>()
                .GetListAsync(predicate: x => x.IsSynced == true);

            if (syncTasks.Count == 0)
            {
                logger.LogInformation("No synced SyncTasks found for deletion.");
                return;
            }

            int count = syncTasks.Count;
            unitOfWork.GetRepository<SyncTask>().DeleteRange(syncTasks);
            await unitOfWork.CommitAsync();

            logger.LogInformation("{Count} synced SyncTasks have been deleted successfully.", count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting synced SyncTasks.");
            throw;
        }
    }

    private async Task MarkAllSyncTaskSynced(string kioskId)
    {
        var syncTasks = await unitOfWork.GetRepository<SyncTask>()
            .GetListAsync(
                predicate: x => x.KioskId == kioskId
            );

        if (syncTasks.Count == 0)
        {
            logger.LogInformation("No pending sync tasks found for kiosk: {KioskId}", kioskId);
            return;
        }

        foreach (var syncTask in syncTasks)
        {
            syncTask.Sync();
        }

        unitOfWork.GetRepository<SyncTask>().UpdateRange(syncTasks);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Updated {Count} sync tasks to IsSynced=true for kiosk: {KioskId}", syncTasks.Count,
            kioskId);
    }
}