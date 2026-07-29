using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum StepType
    {
        // Arm
        MoveArmCommand,
        CloseGripperCommand,
        DiscardCupCommand,
        ResetArmCommand,
        MoveFailCommand,
        OpenFailCommand,
        CloseFailCommand,
        OpenGripperCommand,

        // Coffee
        MakeDrinkCommand,
        MakeFailCommand,

        // Cup Dropping
        DropCupCommand,
        DropFailCommand,

        // Ice
        TakeIceCommand,
        TakeFailCommand,

        // Order
        AlertCancellationCommand,
        CancelOrderCommand,
        CompleteOrderCommand,
        CreateOrderCommand,

        // Payment
        CancelPaymentCommand,
        RefundCommand,
        ValidatePaymentCommand

    }

}
