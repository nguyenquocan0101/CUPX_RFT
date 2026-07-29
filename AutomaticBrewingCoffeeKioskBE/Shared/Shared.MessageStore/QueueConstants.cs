
namespace Shared.MessageStore
{
    public class QueueConstants
    {
        public const string EXCHANGE_NAME = "kiosk";

        public const string QUEUE_WORKFLOW_EXECUTE = "workflow-execute";
        public const string QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY = "workflow.execute";

        public const string QUEUE_DEVICE_UPDATE = "devicedocument";
        public const string QUEUE_DEVICE_UPDATE_ROUTING_KEY = "device.update";

        public const string QUEUE_STEP_UPDATE = "step-update";
        public const string QUEUE_STEP_UPDATE_ROUTING_KEY = "step.update";

        public const string QUEUE_ORDER = "order";
        public const string QUEUE_ORDER_ROUTING_KEY = "order.*";
        public const string QUEUE_ORDER_ROUTING_KEY_UPDATE = "order.update";

        public const string EXCHANGE_DEVICE_COMMAND = "device-command";
        public const string QUEUE_DEVICE_COMMAND = "device-command";
        public const string QUEUE_DEVICE_COMMAND_DLQ = "device-command.dlq";
        public const string ROUTING_DEVICE_COMMAND = "device.command";
    }
}
