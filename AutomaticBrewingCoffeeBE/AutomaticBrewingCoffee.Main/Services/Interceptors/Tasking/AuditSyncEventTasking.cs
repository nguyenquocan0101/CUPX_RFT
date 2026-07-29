using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Services.Interceptors.Tasking;

public class AuditSyncEventTasking        
{
    // Main function using in the interceptor
    public async Task AuditSyncEventAsync(DbContextEventData eventData)
    {
        await TrackKioskAuditAsyncV2(eventData);
        await TrackKioskDeviceAuditAsyncV2(eventData);
        await TrackWorkflowAuditAsyncV2(eventData);
        await TrackStepAuditAsyncV3(eventData);
    }

    private async Task TrackKioskDeviceAuditAsyncV2(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType() == typeof(KioskDeviceMapping))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList()!;

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var kioskDevice = (KioskDeviceMapping)entry.Entity;

            var syncEventType = kioskDevice.IsDeleted
                ? ESyncEventType.Delete
                : entry.State switch
                {
                    EntityState.Added => ESyncEventType.Create,
                    EntityState.Modified => ESyncEventType.Update,
                    EntityState.Deleted => ESyncEventType.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
                };

            // var syncEvent =
            //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == product.ProductId)!;
            //
            // if (syncEvent is not null)
            // {
            //     syncEvent.SyncEventType = syncEventType.ToString();
            //     syncEvent.UpdatedDate = DateTime.UtcNow;
            //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
            //
            //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
            //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
            //         .ToListAsync()!;
            //
            //     if (existSyncTasks.Count == 0)
            //     {
            //         continue;
            //     }
            //
            //     foreach (var existSyncTask in existSyncTasks)
            //     {
            //         existSyncTask.Async();
            //     }
            //
            //     continue;
            // }

            if (kioskDevice.KioskId is null)
            {
                continue;
            }

            var syncEvent = new SyncEvent
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = nameof(KioskDeviceMapping),
                EntityId = kioskDevice.KioskDeviceMappingId,
                SyncEventType = syncEventType.ToString(),
                CreatedDate = DateTime.UtcNow,
                DeletedDate = null,
                UpdatedDate = null,
                IsDeleted = false,
            };

            eventData.Context?.Set<SyncEvent>().Add(syncEvent);

            var syncTask = new SyncTask()
            {
                SyncTaskId = Guid.NewGuid().ToString(),
                SyncEventId = syncEvent.SyncEventId,
                KioskId = kioskDevice.KioskId!,
                SyncEvent = null,
                IsSynced = false,
                SyncedAt = null,
                CreatedDate = DateTime.UtcNow,
            };

            eventData.Context?.Set<SyncTask>().Add(syncTask);

            var syncEventDevice = new SyncEvent
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = nameof(Device),
                EntityId = kioskDevice.DeviceId!,
                SyncEventType = syncEventType.ToString(),
                CreatedDate = DateTime.UtcNow,
                DeletedDate = null,
                UpdatedDate = null,
                IsDeleted = false,
            };

            eventData.Context?.Set<SyncEvent>().Add(syncEventDevice);

            var syncTaskDevice = new SyncTask()
            {
                SyncTaskId = Guid.NewGuid().ToString(),
                SyncEventId = syncEventDevice.SyncEventId,
                KioskId = kioskDevice.KioskId!,
                SyncEvent = null,
                IsSynced = false,
                SyncedAt = null,
                CreatedDate = DateTime.UtcNow,
            };

            eventData.Context?.Set<SyncTask>().Add(syncTaskDevice);
        }
    }

    private async Task TrackKioskAuditAsyncV2(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType() == typeof(Kiosk))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList()!;

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var kiosk = (Kiosk)entry.Entity;

            var syncEventType = kiosk.IsDeleted
                ? ESyncEventType.Delete
                : entry.State switch
                {
                    EntityState.Added => ESyncEventType.Create,
                    EntityState.Modified => ESyncEventType.Update,
                    EntityState.Deleted => ESyncEventType.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
                };

            if (syncEventType == ESyncEventType.Create)
            {
                var kioskVersion = await eventData.Context!.Set<KioskVersion>()
                    .Include(x => x.KioskVersionProductMappings)
                    .FirstAsync(x => x.KioskVersionId == kiosk.KioskVersionId);

                var supportProducts = kioskVersion.KioskVersionProductMappings.ToList();

                foreach (var product in supportProducts)
                {
                    var workflow = await eventData.Context!.Set<Workflow>().Include(x => x.Steps)
                        .FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

                    if (workflow is null)
                    {
                        continue;
                    }

                    var syncEventWorkflow = new SyncEvent()
                    {
                        SyncEventId = Guid.NewGuid().ToString(),
                        EntityType = nameof(Workflow),
                        EntityId = workflow.WorkflowId,
                        SyncEventType = syncEventType.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        DeletedDate = null,
                        UpdatedDate = null,
                        IsDeleted = false,
                    };

                    eventData.Context?.Set<SyncEvent>().Add(syncEventWorkflow);

                    var syncTaskWorkflow = new SyncTask()
                    {
                        SyncTaskId = Guid.NewGuid().ToString(),
                        SyncEventId = syncEventWorkflow.SyncEventId,
                        KioskId = kiosk.KioskId,
                        SyncEvent = null,
                        IsSynced = false,
                        SyncedAt = null,
                        CreatedDate = DateTime.UtcNow,
                    };

                    eventData.Context?.Set<SyncTask>().Add(syncTaskWorkflow);

                    if (workflow.Steps!.Count == 0)
                    {
                        continue;
                    }

                    foreach (var step in workflow.Steps)
                    {
                        var syncEventStep = new SyncEvent()
                        {
                            SyncEventId = Guid.NewGuid().ToString(),
                            EntityType = nameof(Step),
                            EntityId = step.StepId,
                            SyncEventType = syncEventType.ToString(),
                            CreatedDate = DateTime.UtcNow,
                            DeletedDate = null,
                            UpdatedDate = null,
                            IsDeleted = false,
                        };

                        eventData.Context!.Set<SyncEvent>().Add(syncEventStep);

                        var syncTaskStep = new SyncTask()
                        {
                            SyncTaskId = Guid.NewGuid().ToString(),
                            SyncEventId = syncEventStep.SyncEventId,
                            KioskId = kiosk.KioskId,
                            SyncEvent = null,
                            IsSynced = false,
                            SyncedAt = null,
                            CreatedDate = DateTime.UtcNow,
                        };

                        eventData.Context!.Set<SyncTask>().Add(syncTaskStep);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tracking workflow audit V2
    /// </summary>
    /// <param name="eventData"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private async Task TrackWorkflowAuditAsyncV2(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType() == typeof(Workflow))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList()!;

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var workflow = (Workflow)entry.Entity;

            var syncEventType = entry.State switch
            {
                EntityState.Added => ESyncEventType.Create,
                EntityState.Modified => ESyncEventType.Update,
                EntityState.Deleted => ESyncEventType.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
            };

            var syncEvent = new SyncEvent
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = nameof(Workflow),
                EntityId = workflow.WorkflowId,
                SyncEventType = syncEventType.ToString(),
                CreatedDate = DateTime.UtcNow,
                DeletedDate = null,
                UpdatedDate = null,
                IsDeleted = false,
            };

            eventData.Context?.Set<SyncEvent>().Add(syncEvent);

            // If product not exist then do not need to trace kiosk
            if (workflow.ProductId is null)
            {
                continue;
            }

            var kiosks = await eventData.Context!.Set<Kiosk>()
                .Where(x => x.KioskVersionId == workflow.KioskVersionId).ToListAsync();

            foreach (var kiosk in kiosks)
            {
                var syncTask = new SyncTask()
                {
                    SyncTaskId = Guid.NewGuid().ToString(),
                    SyncEventId = syncEvent.SyncEventId,
                    KioskId = kiosk.KioskId,
                    SyncEvent = null,
                    IsSynced = false,
                    SyncedAt = null,
                    CreatedDate = DateTime.UtcNow,
                };

                eventData.Context?.Set<SyncTask>().Add(syncTask);
            }
        }
    }

    /// <summary>
    ///  Tracking step audit V2
    /// </summary>
    /// <param name="eventData"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private async Task TrackStepAuditAsyncV3(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType() == typeof(Step))
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList()!;

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var step = (Step)entry.Entity;

            var syncEventType = entry.State switch
            {
                EntityState.Added => ESyncEventType.Create,
                EntityState.Modified => ESyncEventType.Update,
                EntityState.Deleted => ESyncEventType.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
            };

            await TraverseStepAuditStepAsyncV2(syncEventType, step, new List<Step>(), new HashSet<string>(), eventData);
        }
    }

    private async Task TraverseStepAuditStepAsyncV2(
        ESyncEventType syncEventType,
        Step step,
        List<Step> result,
        HashSet<string> visitedStepIds,
        DbContextEventData eventData
    )
    {
        if (visitedStepIds.Contains(step.StepId))
            return;

        result.Add(step);
        visitedStepIds.Add(step.StepId);

        var syncEventStep = new SyncEvent()
        {
            SyncEventId = Guid.NewGuid().ToString(),
            EntityType = nameof(Step),
            EntityId = step.StepId,
            SyncEventType = syncEventType.ToString(),
            CreatedDate = DateTime.UtcNow,
            DeletedDate = null,
            UpdatedDate = null,
            IsDeleted = false,
        };

        eventData.Context!.Set<SyncEvent>().Add(syncEventStep);

        var workflow = await eventData.Context?.Set<Workflow>().FirstAsync(x => x.WorkflowId == step.WorkflowId)!;

        var kiosks = await eventData.Context!.Set<Kiosk>().Where(x => x.KioskVersionId == workflow.KioskVersionId)
            .ToListAsync();

        var syncTaskSteps = new List<SyncTask>();

        foreach (var kiosk in kiosks)
        {
            var syncTask = new SyncTask
            {
                SyncTaskId = Guid.NewGuid().ToString(),
                SyncEventId = syncEventStep.SyncEventId,
                KioskId = kiosk.KioskId,
                SyncEvent = null,
                IsSynced = false,
                SyncedAt = null,
                CreatedDate = DateTime.UtcNow,
            };
            syncTaskSteps.Add(syncTask);
        }


        eventData.Context?.Set<SyncTask>().AddRange(syncTaskSteps);

        if (!string.IsNullOrEmpty(step.CallbackWorkflowId))
        {
            var syncEventWorkflow = new SyncEvent()
            {
                SyncEventId = Guid.NewGuid().ToString(),
                EntityType = nameof(Workflow),
                EntityId = step.CallbackWorkflowId,
                SyncEventType = syncEventType.ToString(),
                CreatedDate = DateTime.UtcNow,
                DeletedDate = null,
                UpdatedDate = null,
                IsDeleted = false,
            };

            eventData.Context!.Set<SyncEvent>().Add(syncEventWorkflow);

            var syncTaskWorkflows = new List<SyncTask>();

            var callbackWorkflow = await eventData.Context!.Set<Workflow>()
                .FirstOrDefaultAsync(x => x.WorkflowId == step.CallbackWorkflowId);

            if (callbackWorkflow is not null)
            {
                kiosks = await eventData.Context.Set<Kiosk>()
                    .Where(x => x.KioskVersionId == callbackWorkflow.KioskVersionId).ToListAsync();

                foreach (var kiosk in kiosks)
                {
                    var syncTask = new SyncTask
                    {
                        SyncTaskId = Guid.NewGuid().ToString(),
                        SyncEventId = syncEventStep.SyncEventId,
                        KioskId = kiosk.KioskId,
                        SyncEvent = null,
                        IsSynced = false,
                        SyncedAt = null,
                        CreatedDate = DateTime.UtcNow,
                    };
                    syncTaskWorkflows.Add(syncTask);
                }


                eventData.Context?.Set<SyncTask>().AddRange(syncTaskWorkflows);
            }

            var nextStep = await eventData.Context!.Set<Step>().SingleOrDefaultAsync(
                predicate: x => x.WorkflowId == step.CallbackWorkflowId
            );

            if (nextStep != null)
            {
                await TraverseStepAuditStepAsyncV2(syncEventType, nextStep, result, visitedStepIds, eventData);
            }
        }
    }
}