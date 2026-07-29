using Services.Implements;

namespace Services.Utils;

public static class NotificationUtil
{
    private static ELanguage CurrentELanguage { get; set; } = ELanguage.Vietnamese;

    public static void SetELanguage(ELanguage language)
    {
        CurrentELanguage = language;
    }

    private static (string Title, string Message) Translate(string viTitle, string viMessage, string enTitle,
        string enMessage)
    {
        return CurrentELanguage == ELanguage.English
            ? (enTitle, enMessage)
            : (viTitle, viMessage);
    }

    public static (string Title, string Message) OrderExecuteFailed(string orderId, string kioskId)
        => Translate(
            "Pha chế đơn hàng thất bại",
            $"Đơn hàng {orderId} đã được gửi đến kiosk {kioskId} để pha chế nhưng bị lỗi.",
            "Order execution failed",
            $"Order {orderId} was sent to kiosk {kioskId} for brewing but failed."
        );

    public static (string Title, string Message) KioskNotWorking(string kioskId)
        => Translate(
            "Ki-ốt không hoạt động",
            $"Hệ thống không thể kết nối hoặc nhận phản hồi từ ki-ốt {kioskId}. Vui lòng kiểm tra tình trạng hoạt động của ki-ốt.",
            "Kiosk not responding",
            $"The system could not connect to or receive a response from kiosk {kioskId}. Please check the kiosk's status."
        );

    public static (string Title, string Message) KioskNotEnoughIngredient(
        string kioskId,
        List<IngredientHelper.MissingIngredientInfo> missingIngredients
    )
    {
        // Format chi tiết nguyên liệu bị thiếu
        var ingredientDetailsVi = string.Join("\n", missingIngredients.Select(i =>
            $"- {i.IngredientType}: cần {i.Required}, còn {i.Available} (thiết bị: {string.Join(", ", i.DeviceNames)})"
        ));

        var ingredientDetailsEn = string.Join("\n", missingIngredients.Select(i =>
            $"- {i.IngredientType}: required {i.Required}, available {i.Available} (devices: {string.Join(", ", i.DeviceNames)})"
        ));

        return Translate(
            "Ki-ốt thiếu nguyên liệu",
            $"Ki-ốt {kioskId} không đủ nguyên liệu để pha chế đơn hàng:\n{ingredientDetailsVi}\nVui lòng bổ sung nguyên liệu để tiếp tục hoạt động.",
            "Kiosk out of ingredients",
            $"Kiosk {kioskId} does not have enough ingredients to prepare the order:\n{ingredientDetailsEn}\nPlease refill the ingredients to continue operation."
        );
    }

    public static (string Title, string Message) KioskReceiveOrderFailed(string kioskId, string orderId)
        => Translate(
            "Ki-ốt không nhận đơn hàng",
            $"Đơn hàng {orderId} đã được gửi đến ki-ốt {kioskId} để pha chế nhưng không nhận được phản hồi. Vui lòng kiểm tra tình trạng kết nối và hoạt động của ki-ốt.",
            "Kiosk failed to receive order",
            $"Order {orderId} was sent to kiosk {kioskId} for brewing but no response was received. Please check the kiosk's connection and operational status."
        );
}