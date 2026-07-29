namespace Services.Dtos.Sync;
public class SyncActionDto
{
    public ActionDto SyncActions { get; set; }
}

public class ActionDto
{
    public KioskDataSyncDto Actions { get; set; }
}
public class KioskDataSyncDto
{
    public EntitySyncOperation<DeviceSyncDto>? Device { get; set; }
    // public EntitySyncOperation<ProductSyncDto>? Product { get; set; }
    public EntitySyncOperation<WorkflowSyncDto>? Workflow { get; set; }
    public EntitySyncOperation<StepSyncDto>? Step { get; set; }
    //public EntitySyncOperation<MenuSyncDto>? Menu { get; set; }
    //public EntitySyncOperation<MenuProductMappingSyncDto>? MenuProductMapping { get; set; }
}

public class EntitySyncOperation<T>
{
    public List<T>? Create { get; set; }
    public List<T>? Update { get; set; }
    public List<T>? Delete { get; set; }
}


public class OverridenKioskDataSyncDto
{
    public List<DeviceSyncDto>? Devices { get; set; }
    public List<WorkflowSyncDto>? Workflows { get; set; }
    public List<StepSyncDto>? Steps { get; set; }
}