#region OldTrack

// private async Task TrackMenuAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(Menu))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var menu = (Menu)entry.Entity;
//
//         if (menu.KioskId is null)
//         {
//             continue;
//         }
//
//         var syncEventType = menu.IsDeleted
//             ? ESyncEventType.Delete
//             : entry.State switch
//             {
//                 EntityState.Added => ESyncEventType.Create,
//                 EntityState.Modified => ESyncEventType.Update,
//                 EntityState.Deleted => ESyncEventType.Delete,
//                 _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//             };
//
//         // Modify the exist sync event to new state
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == menu.MenuId)!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         return;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     return;
//         // }
//
//         // Create new sync event with the current state
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Menu),
//             EntityId = menu.MenuId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var syncTask = new SyncTask
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEvent.SyncEventId,
//             KioskId = menu.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTask);
//     }
// }
//
// private async Task TrackMenuProductMappingAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(MenuProductMapping))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var menuProductMapping = (MenuProductMapping)entry.Entity;
//
//         var syncEventType = entry.State switch
//         {
//             EntityState.Added => ESyncEventType.Create,
//             EntityState.Modified => ESyncEventType.Update,
//             EntityState.Deleted => ESyncEventType.Delete,
//             _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//         };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>()
//         //         .FirstOrDefaultAsync(
//         //             e => e.EntityId == menuProductMapping.MenuId
//         //                  && e.SecondEntityId == menuProductMapping.ProductId
//         //         )!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(MenuProductMapping),
//             EntityId = menuProductMapping.MenuId,
//             SecondEntityId = menuProductMapping.ProductId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var menu = await eventData.Context?.Set<Menu>()
//             .FirstAsync(e => e.MenuId == menuProductMapping.MenuId)!;
//
//         var syncTask = new SyncTask
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEvent.SyncEventId,
//             KioskId = menu.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTask);
//
//         if (syncEventType != ESyncEventType.Update)
//         {
//             // trace product by menuProductMapping
//             // var syncEventProduct =
//             //     await eventData.Context?.Set<SyncEvent>()
//             //         .FirstOrDefaultAsync(
//             //             e => e.EntityId == menuProductMapping.ProductId
//             //         )!;
//             //
//             // if (syncEventProduct is not null)
//             // {
//             //     syncEventProduct.SyncEventType = syncEventType.ToString();
//             //     syncEventProduct.UpdatedDate = DateTime.UtcNow;
//             //     eventData.Context?.Set<SyncEvent>().Update(syncEventProduct);
//             //
//             //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//             //         .Where(x => x.SyncEventId == syncEventProduct.SyncEventId)
//             //         .ToListAsync()!;
//             //
//             //     if (existSyncTasks.Count == 0)
//             //     {
//             //         continue;
//             //     }
//             //
//             //     foreach (var existSyncTask in existSyncTasks)
//             //     {
//             //         existSyncTask.Async();
//             //     }
//             //
//             //     continue;
//             // }
//
//             // trace product sync event auditing
//             var syncEventProduct = new SyncEvent()
//             {
//                 SyncEventId = Guid.NewGuid().ToString(),
//                 EntityType = nameof(Product),
//                 EntityId = menuProductMapping.ProductId,
//                 SecondEntityId = null,
//                 SyncEventType = syncEventType.ToString(),
//                 CreatedDate = DateTime.UtcNow,
//                 DeletedDate = null,
//                 UpdatedDate = null,
//                 IsDeleted = false,
//             };
//
//             eventData.Context?.Set<SyncEvent>().Add(syncEventProduct);
//
//             // trace product sync task auditing
//             var syncTaskProduct = new SyncTask
//             {
//                 SyncTaskId = Guid.NewGuid().ToString(),
//                 SyncEventId = syncEventProduct.SyncEventId,
//                 KioskId = menu.KioskId!,
//                 SyncEvent = null,
//                 IsSynced = false,
//                 SyncedAt = null,
//                 CreatedAt = DateTime.UtcNow,
//             };
//
//             eventData.Context?.Set<SyncTask>().Add(syncTaskProduct);
//
//             // trace workflow by product
//             var workflows = eventData.Context?.Set<Workflow>().Where(
//                 x => x.ProductId == menuProductMapping.ProductId).ToList();
//
//             if (workflows!.Count > 0)
//             {
//                 foreach (var workflow in workflows)
//                 {
//                     // trace product sync event auditing
//                     var syncEventWorkflow = new SyncEvent()
//                     {
//                         SyncEventId = Guid.NewGuid().ToString(),
//                         EntityType = nameof(Workflow),
//                         EntityId = workflow.WorkflowId,
//                         SecondEntityId = null,
//                         SyncEventType = syncEventType.ToString(),
//                         CreatedDate = DateTime.UtcNow,
//                         DeletedDate = null,
//                         UpdatedDate = null,
//                         IsDeleted = false,
//                     };
//                     eventData.Context!.Add(syncEventWorkflow);
//
//                     // tract workflow sync event auditing
//                     var syncTaskWorkflow = new SyncTask
//                     {
//                         SyncTaskId = Guid.NewGuid().ToString(),
//                         SyncEventId = syncEventWorkflow.SyncEventId,
//                         KioskId = menu.KioskId!,
//                         SyncEvent = null,
//                         IsSynced = false,
//                         SyncedAt = null,
//                         CreatedAt = DateTime.UtcNow,
//                     };
//
//                     eventData.Context!.Add(syncTaskWorkflow);
//
//                     // trace steps by product
//                     var steps = eventData.Context.Set<Step>().Where(x => x.WorkflowId == workflow.WorkflowId)
//                         .ToList();
//
//                     if (steps.Count > 0)
//                     {
//                         foreach (var step in steps)
//                         {
//                             // trace step sync event auditing
//                             var syncEventStep = new SyncEvent()
//                             {
//                                 SyncEventId = Guid.NewGuid().ToString(),
//                                 EntityType = nameof(Step),
//                                 EntityId = step.StepId,
//                                 SecondEntityId = null,
//                                 SyncEventType = syncEventType.ToString(),
//                                 CreatedDate = DateTime.UtcNow,
//                                 DeletedDate = null,
//                                 UpdatedDate = null,
//                                 IsDeleted = false,
//                             };
//                             eventData.Context!.Add(syncEventStep);
//
//                             // trace step sync task auditing
//                             var syncTaskStep = new SyncTask
//                             {
//                                 SyncTaskId = Guid.NewGuid().ToString(),
//                                 SyncEventId = syncEventStep.SyncEventId,
//                                 KioskId = menu.KioskId!,
//                                 SyncEvent = null,
//                                 IsSynced = false,
//                                 SyncedAt = null,
//                                 CreatedAt = DateTime.UtcNow,
//                             };
//                             eventData.Context!.Add(syncTaskStep);
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }

// private async Task TrackMenuProductMappingAuditAsyncV2(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(MenuProductMapping))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var menuProductMapping = (MenuProductMapping)entry.Entity;
//
//         var syncEventType = entry.State switch
//         {
//             EntityState.Added => ESyncEventType.Create,
//             EntityState.Modified => ESyncEventType.Update,
//             EntityState.Deleted => ESyncEventType.Delete,
//             _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//         };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>()
//         //         .FirstOrDefaultAsync(
//         //             e => e.EntityId == menuProductMapping.MenuId
//         //                  && e.SecondEntityId == menuProductMapping.ProductId
//         //         )!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(MenuProductMapping),
//             EntityId = menuProductMapping.MenuId,
//             SecondEntityId = menuProductMapping.ProductId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var menu = await eventData.Context?.Set<Menu>()
//             .FirstAsync(e => e.MenuId == menuProductMapping.MenuId)!;
//
//         var syncTask = new SyncTask
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEvent.SyncEventId,
//             KioskId = menu.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTask);
//
//         if (syncEventType != ESyncEventType.Update)
//         {
//             // trace product by menuProductMapping
//             // var syncEventProduct =
//             //     await eventData.Context?.Set<SyncEvent>()
//             //         .FirstOrDefaultAsync(
//             //             e => e.EntityId == menuProductMapping.ProductId
//             //         )!;
//             //
//             // if (syncEventProduct is not null)
//             // {
//             //     syncEventProduct.SyncEventType = syncEventType.ToString();
//             //     syncEventProduct.UpdatedDate = DateTime.UtcNow;
//             //     eventData.Context?.Set<SyncEvent>().Update(syncEventProduct);
//             //
//             //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//             //         .Where(x => x.SyncEventId == syncEventProduct.SyncEventId)
//             //         .ToListAsync()!;
//             //
//             //     if (existSyncTasks.Count == 0)
//             //     {
//             //         continue;
//             //     }
//             //
//             //     foreach (var existSyncTask in existSyncTasks)
//             //     {
//             //         existSyncTask.Async();
//             //     }
//             //
//             //     continue;
//             // }
//
//             // trace product sync event auditing
//             var syncEventProduct = new SyncEvent()
//             {
//                 SyncEventId = Guid.NewGuid().ToString(),
//                 EntityType = nameof(Product),
//                 EntityId = menuProductMapping.ProductId,
//                 SecondEntityId = null,
//                 SyncEventType = syncEventType.ToString(),
//                 CreatedDate = DateTime.UtcNow,
//                 DeletedDate = null,
//                 UpdatedDate = null,
//                 IsDeleted = false,
//             };
//
//             eventData.Context?.Set<SyncEvent>().Add(syncEventProduct);
//
//             // trace product sync task auditing
//             var syncTaskProduct = new SyncTask
//             {
//                 SyncTaskId = Guid.NewGuid().ToString(),
//                 SyncEventId = syncEventProduct.SyncEventId,
//                 KioskId = menu.KioskId!,
//                 SyncEvent = null,
//                 IsSynced = false,
//                 SyncedAt = null,
//                 CreatedAt = DateTime.UtcNow,
//             };
//
//             eventData.Context?.Set<SyncTask>().Add(syncTaskProduct);
//
//             // trace workflow by product
//             var workflows = eventData.Context?.Set<Workflow>().Where(
//                 x => x.ProductId == menuProductMapping.ProductId).ToList();
//
//             if (workflows!.Count > 0)
//             {
//                 foreach (var workflow in workflows)
//                 {
//                     // trace product sync event auditing
//                     var syncEventWorkflow = new SyncEvent()
//                     {
//                         SyncEventId = Guid.NewGuid().ToString(),
//                         EntityType = nameof(Workflow),
//                         EntityId = workflow.WorkflowId,
//                         SecondEntityId = null,
//                         SyncEventType = syncEventType.ToString(),
//                         CreatedDate = DateTime.UtcNow,
//                         DeletedDate = null,
//                         UpdatedDate = null,
//                         IsDeleted = false,
//                     };
//                     eventData.Context!.Add(syncEventWorkflow);
//
//                     // tract workflow sync event auditing
//                     var syncTaskWorkflow = new SyncTask
//                     {
//                         SyncTaskId = Guid.NewGuid().ToString(),
//                         SyncEventId = syncEventWorkflow.SyncEventId,
//                         KioskId = menu.KioskId!,
//                         SyncEvent = null,
//                         IsSynced = false,
//                         SyncedAt = null,
//                         CreatedAt = DateTime.UtcNow,
//                     };
//
//                     eventData.Context!.Add(syncTaskWorkflow);
//
//                     // trace steps by product
//                     var steps = eventData.Context.Set<Step>().Where(x => x.WorkflowId == workflow.WorkflowId)
//                         .ToList();
//
//                     if (steps.Count > 0)
//                     {
//                         foreach (var step in steps)
//                         {
//                             await TraverseStepAuditWorkflowAsync(menu.KioskId!, syncEventType, step,
//                                 new List<Step>(),
//                                 new HashSet<string>(),
//                                 eventData);
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }

// private async Task TrackProductAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(Product))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var product = (Product)entry.Entity;
//
//         var syncEventType = product.IsDeleted
//             ? ESyncEventType.Delete
//             : entry.State switch
//             {
//                 EntityState.Added => ESyncEventType.Create,
//                 EntityState.Modified => ESyncEventType.Update,
//                 EntityState.Deleted => ESyncEventType.Delete,
//                 _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//             };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == product.ProductId)!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Menu),
//             EntityId = product.ProductId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var menuProductMappings = await eventData.Context?.Set<MenuProductMapping>()
//             .Where(x => x.ProductId == product.ProductId)
//             .ToListAsync()!;
//
//         var syncTasks = new List<SyncTask>();
//
//         foreach (var menuProductMapping in menuProductMappings)
//         {
//             var menus = await eventData.Context?.Set<Menu>()
//                 .AsNoTracking()
//                 .Where(x => x.MenuId == menuProductMapping.MenuId)
//                 .ToListAsync()!;
//
//             foreach (var menu in menus)
//             {
//                 var syncTask = new SyncTask
//                 {
//                     SyncTaskId = Guid.NewGuid().ToString(),
//                     SyncEventId = syncEvent.SyncEventId,
//                     KioskId = menu.KioskId!,
//                     SyncEvent = null,
//                     IsSynced = false,
//                     SyncedAt = null,
//                     CreatedAt = DateTime.UtcNow,
//                 };
//                 syncTasks.Add(syncTask);
//             }
//         }
//
//         eventData.Context?.Set<SyncTask>().AddRange(syncTasks);
//     }
// }
//
// private async Task TrackWorkflowAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(Workflow))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var workflow = (Workflow)entry.Entity;
//
//         var syncEventType = entry.State switch
//         {
//             EntityState.Added => ESyncEventType.Create,
//             EntityState.Modified => ESyncEventType.Update,
//             EntityState.Deleted => ESyncEventType.Delete,
//             _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//         };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == workflow.WorkflowId)!;
//         //
//         // // If sync event exist then trace the tasks and audit its then break the loop
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Workflow),
//             EntityId = workflow.WorkflowId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         // If product not exist then do not need to trace kiosk
//         if (workflow.ProductId is null)
//         {
//             continue;
//         }
//
//         var menuProductMappings = await eventData.Context?.Set<MenuProductMapping>()
//             .Where(x => x.ProductId == workflow.ProductId)
//             .ToListAsync()!;
//
//         var syncTasks = new List<SyncTask>();
//
//         foreach (var menuProductMapping in menuProductMappings)
//         {
//             var menus = await eventData.Context?.Set<Menu>()
//                 .Where(x => x.MenuId == menuProductMapping.MenuId)
//                 .ToListAsync()!;
//
//             foreach (var menu in menus)
//             {
//                 var syncTask = new SyncTask
//                 {
//                     SyncTaskId = Guid.NewGuid().ToString(),
//                     SyncEventId = syncEvent.SyncEventId,
//                     KioskId = menu.KioskId!,
//                     SyncEvent = null,
//                     IsSynced = false,
//                     SyncedAt = null,
//                     CreatedAt = DateTime.UtcNow,
//                 };
//                 syncTasks.Add(syncTask);
//             }
//         }
//
//         eventData.Context?.Set<SyncTask>().AddRange(syncTasks);
//     }
// }

// private async Task TrackStepAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(Step))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var step = (Step)entry.Entity;
//
//         var syncEventType = entry.State switch
//         {
//             EntityState.Added => ESyncEventType.Create,
//             EntityState.Modified => ESyncEventType.Update,
//             EntityState.Deleted => ESyncEventType.Delete,
//             _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//         };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == step.StepId)!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Step),
//             EntityId = step.StepId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var workflow = await eventData.Context?.Set<Workflow>().FirstAsync(x => x.WorkflowId == step.WorkflowId)!;
//
//         var menuProductMappings = await eventData.Context?.Set<MenuProductMapping>()
//             .Where(x => x.ProductId == workflow.ProductId)
//             .ToListAsync()!;
//
//         var syncTasks = new List<SyncTask>();
//
//         foreach (var menuProductMapping in menuProductMappings)
//         {
//             var menus = await eventData.Context?.Set<Menu>()
//                 .Where(x => x.MenuId == menuProductMapping.MenuId)
//                 .ToListAsync()!;
//
//             foreach (var menu in menus)
//             {
//                 var syncTask = new SyncTask
//                 {
//                     SyncTaskId = Guid.NewGuid().ToString(),
//                     SyncEventId = syncEvent.SyncEventId,
//                     KioskId = menu.KioskId!,
//                     SyncEvent = null,
//                     IsSynced = false,
//                     SyncedAt = null,
//                     CreatedAt = DateTime.UtcNow,
//                 };
//                 syncTasks.Add(syncTask);
//             }
//         }
//
//         eventData.Context?.Set<SyncTask>().AddRange(syncTasks);
//     }
// }

// private async Task TrackKioskAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(Kiosk))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var kiosk = (Kiosk)entry.Entity;
//
//         var syncEventType = kiosk.IsDeleted
//             ? ESyncEventType.Delete
//             : entry.State switch
//             {
//                 EntityState.Added => ESyncEventType.Create,
//                 EntityState.Modified => ESyncEventType.Update,
//                 EntityState.Deleted => ESyncEventType.Delete,
//                 _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//             };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == product.ProductId)!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Kiosk),
//             EntityId = kiosk.KioskId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var syncTask = new SyncTask()
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEvent.SyncEventId,
//             KioskId = kiosk.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTask);
//     }
// }

// private async Task TrackKioskDeviceAuditAsync(DbContextEventData eventData)
// {
//     var entries = eventData.Context?.ChangeTracker.Entries()
//         .Where(e => e.Entity.GetType() == typeof(KioskDeviceMapping))
//         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//         .ToList()!;
//
//     if (entries.Count == 0)
//     {
//         return;
//     }
//
//     foreach (var entry in entries)
//     {
//         var kioskDevice = (KioskDeviceMapping)entry.Entity;
//
//         var syncEventType = kioskDevice.IsDeleted
//             ? ESyncEventType.Delete
//             : entry.State switch
//             {
//                 EntityState.Added => ESyncEventType.Create,
//                 EntityState.Modified => ESyncEventType.Update,
//                 EntityState.Deleted => ESyncEventType.Delete,
//                 _ => throw new ArgumentOutOfRangeException(nameof(entry.State), entry.State, null)
//             };
//
//         // var syncEvent =
//         //     await eventData.Context?.Set<SyncEvent>().FirstOrDefaultAsync(e => e.EntityId == product.ProductId)!;
//         //
//         // if (syncEvent is not null)
//         // {
//         //     syncEvent.SyncEventType = syncEventType.ToString();
//         //     syncEvent.UpdatedDate = DateTime.UtcNow;
//         //     eventData.Context?.Set<SyncEvent>().Update(syncEvent);
//         //
//         //     var existSyncTasks = await eventData.Context?.Set<SyncTask>()
//         //         .Where(x => x.SyncEventId == syncEvent.SyncEventId)
//         //         .ToListAsync()!;
//         //
//         //     if (existSyncTasks.Count == 0)
//         //     {
//         //         continue;
//         //     }
//         //
//         //     foreach (var existSyncTask in existSyncTasks)
//         //     {
//         //         existSyncTask.Async();
//         //     }
//         //
//         //     continue;
//         // }
//
//         if (kioskDevice.KioskId is null)
//         {
//             continue;
//         }
//
//         var syncEvent = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(KioskDeviceMapping),
//             EntityId = kioskDevice.KioskDeviceMappingId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEvent);
//
//         var syncTask = new SyncTask()
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEvent.SyncEventId,
//             KioskId = kioskDevice.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTask);
//
//         var syncEventDevice = new SyncEvent
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Device),
//             EntityId = kioskDevice.DeviceId!,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context?.Set<SyncEvent>().Add(syncEventDevice);
//
//         var syncTaskDevice = new SyncTask()
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEventDevice.SyncEventId,
//             KioskId = kioskDevice.KioskId!,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTaskDevice);
//     }
// }


// private async Task TraverseStepAuditStepAsync(
//     ESyncEventType syncEventType,
//     Step step,
//     List<Step> result,
//     HashSet<string> visitedStepIds,
//     DbContextEventData eventData)
// {
//     if (visitedStepIds.Contains(step.StepId))
//         return;
//
//     result.Add(step);
//     visitedStepIds.Add(step.StepId);
//
//     var syncEventStep = new SyncEvent()
//     {
//         SyncEventId = Guid.NewGuid().ToString(),
//         EntityType = nameof(Step),
//         EntityId = step.StepId,
//         SyncEventType = syncEventType.ToString(),
//         CreatedDate = DateTime.UtcNow,
//         DeletedDate = null,
//         UpdatedDate = null,
//         IsDeleted = false,
//     };
//
//     eventData.Context!.Set<SyncEvent>().Add(syncEventStep);
//
//     var workflow = await eventData.Context?.Set<Workflow>().FirstAsync(x => x.WorkflowId == step.WorkflowId)!;
//
//     var menuProductMappingSteps = await eventData.Context?.Set<MenuProductMapping>()
//         .Where(x => x.ProductId == workflow.ProductId)
//         .ToListAsync()!;
//
//     var syncTaskSteps = new List<SyncTask>();
//
//     foreach (var menuProductMapping in menuProductMappingSteps)
//     {
//         var menus = await eventData.Context?.Set<Menu>()
//             .Where(x => x.MenuId == menuProductMapping.MenuId)
//             .ToListAsync()!;
//
//         foreach (var menu in menus)
//         {
//             var kiosks = await eventData.Context.Set<Kiosk>().Where(x => x.MenuId == menu.MenuId).ToListAsync();
//
//             var kioskIds = kiosks.Select(x => x.KioskId);
//
//             foreach (var kioskId in kioskIds)
//             {
//                 var syncTask = new SyncTask
//                 {
//                     SyncTaskId = Guid.NewGuid().ToString(),
//                     SyncEventId = syncEventStep.SyncEventId,
//                     KioskId = kioskId!,
//                     SyncEvent = null,
//                     IsSynced = false,
//                     SyncedAt = null,
//                     CreatedAt = DateTime.UtcNow,
//                 };
//                 syncTaskSteps.Add(syncTask);
//             }
//         }
//     }
//
//     eventData.Context?.Set<SyncTask>().AddRange(syncTaskSteps);
//
//     if (!string.IsNullOrEmpty(step.CallbackWorkflowId))
//     {
//         var syncEventWorkflow = new SyncEvent()
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Workflow),
//             EntityId = step.CallbackWorkflowId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context!.Set<SyncEvent>().Add(syncEventWorkflow);
//
//         var menuProductMappingWorkflows = await eventData.Context?.Set<MenuProductMapping>()
//             .Where(x => x.ProductId == workflow.ProductId)
//             .ToListAsync()!;
//
//         var syncTaskWorkflows = new List<SyncTask>();
//
//         foreach (var menuProductMapping in menuProductMappingWorkflows)
//         {
//             var menus = await eventData.Context?.Set<Menu>()
//                 .Where(x => x.MenuId == menuProductMapping.MenuId)
//                 .ToListAsync()!;
//
//             foreach (var menu in menus)
//             {
//                 var syncTaskWorkflow = new SyncTask
//                 {
//                     SyncTaskId = Guid.NewGuid().ToString(),
//                     SyncEventId = syncEventStep.SyncEventId,
//                     KioskId = menu.KioskId!,
//                     SyncEvent = null,
//                     IsSynced = false,
//                     SyncedAt = null,
//                     CreatedAt = DateTime.UtcNow,
//                 };
//                 syncTaskWorkflows.Add(syncTaskWorkflow);
//             }
//         }
//
//         eventData.Context?.Set<SyncTask>().AddRange(syncTaskWorkflows);
//
//         var nextStep = await eventData.Context!.Set<Step>().SingleOrDefaultAsync(
//             predicate: x => x.WorkflowId == step.CallbackWorkflowId
//         );
//
//         if (nextStep != null)
//         {
//             await TraverseStepAuditStepAsync(syncEventType, nextStep, result, visitedStepIds, eventData);
//         }
//     }
// }

//     private async Task TraverseStepAuditWorkflowAsync(
//     string kioskId,
//     ESyncEventType syncEventType,
//     Step step,
//     List<Step> result,
//     HashSet<string> visitedStepIds,
//     DbContextEventData eventData)
// {
//     if (visitedStepIds.Contains(step.StepId))
//         return;
//
//     result.Add(step);
//     visitedStepIds.Add(step.StepId);
//
//     var syncEventStep = new SyncEvent()
//     {
//         SyncEventId = Guid.NewGuid().ToString(),
//         EntityType = nameof(Step),
//         EntityId = step.StepId,
//         SyncEventType = syncEventType.ToString(),
//         CreatedDate = DateTime.UtcNow,
//         DeletedDate = null,
//         UpdatedDate = null,
//         IsDeleted = false,
//     };
//
//     eventData.Context!.Set<SyncEvent>().Add(syncEventStep);
//
//
//     var syncTaskStep = new SyncTask
//     {
//         SyncTaskId = Guid.NewGuid().ToString(),
//         SyncEventId = syncEventStep.SyncEventId,
//         KioskId = kioskId,
//         SyncEvent = null,
//         IsSynced = false,
//         SyncedAt = null,
//         CreatedAt = DateTime.UtcNow,
//     };
//
//     eventData.Context?.Set<SyncTask>().Add(syncTaskStep);
//
//     if (!string.IsNullOrEmpty(step.CallbackWorkflowId))
//     {
//         var syncEventWorkflow = new SyncEvent()
//         {
//             SyncEventId = Guid.NewGuid().ToString(),
//             EntityType = nameof(Workflow),
//             EntityId = step.CallbackWorkflowId,
//             SyncEventType = syncEventType.ToString(),
//             CreatedDate = DateTime.UtcNow,
//             DeletedDate = null,
//             UpdatedDate = null,
//             IsDeleted = false,
//         };
//
//         eventData.Context!.Set<SyncEvent>().Add(syncEventWorkflow);
//
//
//         var syncTaskWorkflow = new SyncTask
//         {
//             SyncTaskId = Guid.NewGuid().ToString(),
//             SyncEventId = syncEventStep.SyncEventId,
//             KioskId = kioskId,
//             SyncEvent = null,
//             IsSynced = false,
//             SyncedAt = null,
//             CreatedAt = DateTime.UtcNow,
//         };
//
//         eventData.Context?.Set<SyncTask>().Add(syncTaskWorkflow);
//
//         var nextSteps = await eventData.Context!.Set<Step>().Where(
//             predicate: x => x.WorkflowId == step.CallbackWorkflowId
//         ).ToListAsync();
//
//         foreach (var nextStep in nextSteps)
//         {
//             await TraverseStepAuditWorkflowAsync(
//                 kioskId,
//                 syncEventType,
//                 nextStep,
//                 new List<Step>(),
//                 new HashSet<string>(),
//                 eventData
//             );
//         }
//     }
// }

#endregion

