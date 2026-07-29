
namespace Shared.MessageStore
{
   public record DeviceLabelMessage(string DeviceId, Dictionary<string, string> Labels);
}
