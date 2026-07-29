
using Domain.Models;

namespace Domain
{
    public class MessageRecords
    {
        //pre-processing messages
        public record WorkflowExecuteMsg(string OrderId, int Side, Workflow W);
        public record ExecuteCleanWorkflowMsg(Workflow W);


        //In-process messages
        public record UpdateWorkflowStateMsg(string DocId, int NewSate,  List<string> CurrentIdList, bool IsComplete = false);
        public record UpdateStepObservedMsg(string DocId, string StepId, bool Observed, string? Message = null);
        public record UpdateCallbackStepObservedMsg(string DocId, string StepId, bool Observed, string? Message = null);
        public record UnlockDeviceMsg(string DeviceId);

        //In-Reseting process messages
        public record SetReadyCallbackMsg(string DocId, string StepId, string Executor);
        public record ResetForNextCallbackStepMsg(string DocId, string CurrentStepId);

        //post-processing messages
        public record FinishProductMsg(string OrderId, string ProductId, string FinishTime);
        public record FailProductMsg(string OrderId, string ProductId, string FailTime, string Message);
    }
}
