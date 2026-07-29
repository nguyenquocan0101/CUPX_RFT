namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EStepType
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

    // Summary
    AlertCancellationCommand,
    CancelOrderCommand,
    CompleteOrderCommand,
    CreateOrderCommand,

    // Payment
    CancelPaymentCommand,
    RefundCommand,
    ValidatePaymentCommand
}