using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.BackgroundJobs;

public class SyncEventSyncedJob(IUnitOfWork unitOfWork, ILogger<SyncEventSyncedJob> logger)
{
    public async Task SyncEventSyncedAutomatically()
    {
        logger.LogInformation("Starting background job: SyncEventSyncedAutomatically at {Time}", DateTime.UtcNow);
        await CheckSyncEventSynced();
        logger.LogInformation("Finished background job: SyncEventSyncedAutomatically at {Time}", DateTime.UtcNow);
    }

    private async Task CheckSyncEventSynced()
    {
        logger.LogInformation("Checking for orphaned SyncEvents to delete...");

        var syncEvents = await unitOfWork.GetRepository<SyncEvent>().GetListAsync();
        var toDeleteSyncEvents = new List<SyncEvent>();
        
        foreach (var syncEvent in syncEvents)
        {
            var syncTasks = await unitOfWork.GetRepository<SyncTask>().GetListAsync(
                predicate: x => x.SyncEventId == syncEvent.SyncEventId);

            if (syncTasks.Count == 0)
            {
                toDeleteSyncEvents.Add(syncEvent);
                logger.LogInformation("Marked SyncEvent {SyncEventId} for deletion (no associated SyncTasks)", syncEvent.SyncEventId);
            }
        }

        logger.LogInformation("Total SyncEvents marked for deletion: {Count}", toDeleteSyncEvents.Count);

        unitOfWork.GetRepository<SyncEvent>().DeleteRange(toDeleteSyncEvents);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Deleted {Count} SyncEvents successfully", toDeleteSyncEvents.Count);
    }
}