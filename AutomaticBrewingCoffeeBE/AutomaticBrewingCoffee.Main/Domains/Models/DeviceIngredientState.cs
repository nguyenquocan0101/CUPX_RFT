using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;

namespace AutomaticBrewingCoffee.Domain.Models;

public class DeviceIngredientState : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceIngredientStateId { get; set; } = null!;

    [StringLength(50)] public string DeviceId { get; set; } = null!;

    [ForeignKey(nameof(DeviceId))] public Device? Device { get; set; }

    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    [StringLength(100)] [Required] public string IngredientType { get; set; } = null!;

    public double CurrentCapacity { get; set; }

    [StringLength(10)] public string CapacityLevel { get; set; } = null!;

    [StringLength(20)] public string Unit { get; set; } = null!;

    public bool IsWarning { get; set; }

    // Is the device support regenerate this ingredient
    public bool IsRenewable { get; set; }

    // Is this ingredient is the primary ingredient support of the device
    public bool IsPrimary { get; set; }

    public DateTime? LastRefilledDate { get; set; }

    /// <summary>
    /// Cộng hoặc trừ nguyên liệu. Nếu vượt quá max hoặc âm thì tự điều chỉnh lại.
    /// </summary>
    public void ApplyDelta(double deltaAmount)
    {
        // Cập nhật lượng nguyên liệu
        CurrentCapacity += deltaAmount;

        if (CurrentCapacity > MaxCapacity)
        {
            CurrentCapacity = MaxCapacity;
        }

        if (CurrentCapacity < 0)
        {
            CurrentCapacity = 0;
        }

        // Cảnh báo nếu thấp hơn WarningPercent
        var warningThreshold = MaxCapacity * (WarningPercent / 100.0);
        IsWarning = CurrentCapacity < warningThreshold;

        // Xác định mức độ CapacityLevel
        if (CurrentCapacity <= MinCapacity || CurrentCapacity == 0)
        {
            CapacityLevel = ECapacityLevel.Empty.ToString();
        }
        else if (CurrentCapacity <= warningThreshold && CurrentCapacity >= MinCapacity)
        {
            CapacityLevel = ECapacityLevel.Low.ToString();
        }
        else if (CurrentCapacity >= warningThreshold && CurrentCapacity < MaxCapacity)
        {
            CapacityLevel = ECapacityLevel.Medium.ToString();
        }
        else
        {
            CapacityLevel = ECapacityLevel.High.ToString();
        }
    }

    /// <summary>
    /// Tính toán lại trạng thái sao khi cập nhật
    /// </summary>
    public void Recalculate()
    {
        // Đảm bảo CurrentCapacity nằm trong khoảng 0 - MaxCapacity
        if (CurrentCapacity > MaxCapacity)
        {
            CurrentCapacity = MaxCapacity;
        }

        if (CurrentCapacity < 0)
        {
            CurrentCapacity = 0;
        }

        // Cảnh báo nếu thấp hơn WarningPercent (nếu có MaxCapacity)
        var warningThreshold = MaxCapacity > 0 ? MaxCapacity * (WarningPercent / 100.0) : 0;
        IsWarning = CurrentCapacity <= warningThreshold;

        // Xác định mức độ CapacityLevel
        if (CurrentCapacity <= MinCapacity || CurrentCapacity == 0)
        {
            CapacityLevel = ECapacityLevel.Empty.ToString();
        }
        else if (CurrentCapacity <= warningThreshold && CurrentCapacity >= MinCapacity)
        {
            CapacityLevel = ECapacityLevel.Low.ToString();
        }
        else if (CurrentCapacity >= warningThreshold && CurrentCapacity < MaxCapacity)
        {
            CapacityLevel = ECapacityLevel.Medium.ToString();
        }
        else
        {
            CapacityLevel = ECapacityLevel.High.ToString();
        }
    }
}