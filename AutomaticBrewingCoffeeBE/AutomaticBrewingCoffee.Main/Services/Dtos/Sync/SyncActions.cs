namespace Services.Dtos.Sync;

public class SyncActions
{
    public Dictionary<string, object> Actions { get; set; }

    public SyncActions()
    {
        Actions = new Dictionary<string, object>();
    }

    // Thêm entityType vào Actions
    public void AddEntityType<T>(string entityType)
    {
        Actions[entityType] = new SyncAction<T>();
    }

    // Lấy SyncAction cho một entityType cụ thể
    public SyncAction<T> GetSyncAction<T>(string entityType)
    {
        if (!Actions.ContainsKey(entityType))
        {
            AddEntityType<T>(entityType); // Nếu entityType chưa có, tạo mới
        }

        return (SyncAction<T>)Actions[entityType];
    }
}