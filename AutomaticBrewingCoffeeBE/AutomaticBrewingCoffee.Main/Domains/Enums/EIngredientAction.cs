namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EIngredientAction
{
    Consumed = 1, // Thiết bị tiêu hao trong quá trình pha chế
    Refill = 2, // Được nạp lại thủ công hoặc tự động
    Restore = 3, // Khôi phục lại sau khi đơn hàng thất bại
    ManualAdjust = 4 // Thay đổi thủ công từ admin
}