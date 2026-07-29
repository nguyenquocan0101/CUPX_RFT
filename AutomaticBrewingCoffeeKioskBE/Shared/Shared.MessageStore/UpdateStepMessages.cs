
namespace Shared.MessageStore
{
    public record UpdateStepStateMessages(string DocId, string StepId, int State);
    public record UpdateStatusStepMsg(string DeviceId, Dictionary<string, object> status); //UpdateDeviceStatusMessage
}
