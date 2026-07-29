namespace Kiosk.ApiService.Saga
{
    public static class TopicNames
    {
       public const string Payment = "payment";
       public const string Order = "order";
        /*
          public const string Arm = "arm";
         public const string CupDropping = "cup-dropping";
         public const string Coffee = "coffee";
         public const string Ice = "ice";
         */
        public const string Workflow = "workflow";
    }

    public static class QueueNames
    {
        public const string Payment = "payment";
        public const string Order = "order";
        //public const string Workflow = "workflow";
    }

    public static class TopicNameEndpoints
    {
        // Order
        public const string CreateOrder = "create-order";
        public const string CompleteOrder = "complete-order";
        public const string CancelOrder = "cancel-order";
        public const string AlertCancellation = "alert-cancellation";
        public const string OrderCreated = "order-created";
        public const string OrderCompleted = "order-completed";
        public const string OrderCancelled = "order-cancelled";

        // Payment
        public const string ValidatePayment = "validate-payment";
        public const string PaymentPaid = "payment-paid";
        public const string Refund = "refund";
        public const string CancelPayment = "cancel-payment";
        public const string PaymentCreated = "payment-created";
        public const string PaymentCancelled = "payment-cancelled";
        public const string RefundCompleted = "refund-completed";
        public const string ResetArm = "reset-arm";
        public const string ArmReset = "arm-reset";

        /*
          // Arm
         public const string CloseFail = "close-fail";
         public const string CloseGripper = "close-gripper";
         public const string DiscardCup = "discard-cup";
         public const string MoveArm = "move-arm";
         public const string MoveFail = "move-fail";
         public const string OpenFail = "open-fail";
         public const string OpenGripper = "open-gripper";
         public const string ResetArm = "reset-arm";
         public const string ArmMoved = "arm-moved";
         public const string ArmReset = "arm-reset";
         public const string CupDiscarded = "cup-discarded";
         public const string GripperClosed = "gripper-closed";
         public const string GripperOpened = "gripper-opened";

         // Cup Dropping
         public const string DropCup = "drop-cup";
         public const string DropFail = "drop-fail";
         public const string CupDropped = "cup-dropped";

         // Coffee
         public const string MakeDrink = "make-drink";
         public const string MakeFail = "make-fail";
         public const string DrinkMade = "drink-made";

         // Ice
         public const string TakeIce = "take-ice";
         public const string TakeFail = "take-fail";
         public const string IceTaken = "ice-taken";
         */

        // Workflow
        public const string DoWorkflow = "do-workflow";
        public const string WorkflowDone = "workflow-done";
        public const string WorkflowFail = "workflow-fail";

    }

    public static class GroupIds
    {
        public const string Payment = "payment-group";
        public const string Order = "order-group";
        public const string Arm = "arm-group";
        /*
          public const string Arm = "arm-group";
         public const string CupDropping = "cup-dropping-group";
         public const string Coffee = "coffee-group";
         public const string Ice = "ice-group";
         */
        public const string Workflow = "workflow-group";

    }
}
