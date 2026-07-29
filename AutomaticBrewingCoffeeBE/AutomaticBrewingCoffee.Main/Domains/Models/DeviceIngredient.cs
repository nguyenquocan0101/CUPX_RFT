using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class DeviceIngredient : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceIngredientId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [StringLength(100)] [Required] public string Label { get; set; } = null!;

    [StringLength(100)] [Required] public string IngredientType { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    [StringLength(20)] public string Unit { get; set; } = null!;

    // Is the device support regenerate this ingredient
    public bool IsRenewable { get; set; }

    // Is this ingredient is the primary ingredient support of the device
    public bool IsPrimary { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    [ForeignKey(nameof(DeviceModelId))] public DeviceModel? DeviceModel { get; set; }


    /// <summary>
    /// Tên hàm sẽ ảnh hưởng gây tiêu hao loại nguyên liệu này
    /// </summary>
    [StringLength(100)] [Required] public string? DeviceFunctionName { get; set; } = null!;

    /// <summary>
    /// Tên tham số định danh nguyên liệu (vd: "type", "type1", "ingredientCode")
    /// Nếu không có thì để null (TH1).
    /// </summary>
    [StringLength(100)]
    public string? IngredientSelectorParameter { get; set; }

    /// <summary>
    /// Giá trị của tham số trên để ánh xạ đúng nguyên liệu (vd: "1", "ice", "milk")
    /// </summary>
    [StringLength(100)]
    public string? IngredientSelectorValue { get; set; }

    /// <summary>
    /// Tên tham số đại diện cho **lượng nguyên liệu** cần override
    /// </summary>
    [StringLength(100)]
    public string? TargetOverrideParameter { get; set; } = null!;
}