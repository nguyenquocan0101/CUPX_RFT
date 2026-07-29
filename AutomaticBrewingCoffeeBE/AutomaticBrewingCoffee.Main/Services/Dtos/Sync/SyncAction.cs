namespace Services.Dtos.Sync;

public class SyncAction<T>
{
    public List<T> Create { get; set; }
    public List<T> Update { get; set; }
    public List<T> Delete { get; set; }

    public SyncAction()
    {
        Create = new List<T>();
        Update = new List<T>();
        Delete = new List<T>();
    }
}