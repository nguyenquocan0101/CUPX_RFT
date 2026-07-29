using System.Collections.Concurrent;

namespace Services.Interfaces;

public interface IWorkflowDeliveryTracker
{
    void Register(ulong deliveryTag);
    bool TryTake(ulong deliveryTag);
}

public sealed class WorkflowDeliveryTracker : IWorkflowDeliveryTracker
{
    private readonly ConcurrentDictionary<ulong, byte> _deliveryTags = new();

    public void Register(ulong deliveryTag)
    {
        if (deliveryTag != 0)
            _deliveryTags[deliveryTag] = 0;
    }

    public bool TryTake(ulong deliveryTag)
    {
        return deliveryTag != 0 && _deliveryTags.TryRemove(deliveryTag, out _);
    }
}
