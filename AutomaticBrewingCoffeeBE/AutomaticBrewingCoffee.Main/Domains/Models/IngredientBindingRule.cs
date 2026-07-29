using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class IngredientBindingRule
{
    [Required] [Key] [StringLength(50)] public string IngredientBindingRuleId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceIngredientId { get; set; } = null!;

    [ForeignKey(nameof(DeviceIngredientId))]
    public DeviceIngredient? DeviceIngredient { get; set; }

    [StringLength(100)] [Required] public string DeviceFunctionName { get; set; } = null!;

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
    public string TargetOverrideParameter { get; set; } = null!;
}