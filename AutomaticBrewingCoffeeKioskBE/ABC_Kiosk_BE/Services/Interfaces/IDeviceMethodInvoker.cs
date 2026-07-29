using Shared.MessageStore;

namespace Services.Interfaces;

public interface IDeviceMethodInvoker
{
    Task<DeviceCommandResult> InvokeAsync(
        DeviceCommandRequest request,
        CancellationToken cancellationToken = default);
}
