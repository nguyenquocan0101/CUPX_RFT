namespace AutomaticBrewingCoffee.Domain.Enums;

public enum ENotificationType
{
    // Kiosk
    KioskNotWorking,
    KioskBusy,
    KioskNotEnoughIngredient,
    KioskReceiveOrderFailed,

    // Order
    OrderCreateFailed,
    OrderExecuteFailed
}