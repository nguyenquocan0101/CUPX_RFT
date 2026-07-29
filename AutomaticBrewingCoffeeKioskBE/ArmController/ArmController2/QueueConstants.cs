

namespace ArmController2
{
    public class UpdateStepStateMessages
    {
       
        public UpdateStepStateMessages(string docId, string stepId, int state)
        {
            DocId = docId;
            StepId = stepId;
            State = state;
        }
        public string DocId { get; set; }
        public string StepId { get; set; }
        public int State { get; set; }
    }


    internal class QueueConstants
    {

        public const string EXCHANGE_NAME = "kiosk";

        public const string QUEUE_WORKFLOW_EXECUTE = "workflow-execute";
        public const string QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY = "workflow.execute";

        public const string QUEUE_DEVICE_UPDATE = "devicedocument";
        public const string QUEUE_DEVICE_UPDATE_ROUTING_KEY = "device.update";

        public const string QUEUE_STEP_UPDATE = "step-update";
        public const string QUEUE_STEP_UPDATE_ROUTING_KEY = "step.update";
    }

}
