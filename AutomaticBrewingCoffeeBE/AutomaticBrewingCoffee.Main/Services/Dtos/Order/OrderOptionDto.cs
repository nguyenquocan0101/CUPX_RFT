namespace Services.Dtos.Order;

public class OrderOptionDto
{
    public string? DeviceModelId { get; set; } = null!;

    /// <summary>
    /// Point to the ingredient type
    /// </summary>
    public string? Target { get; set; } = null!;

    /// <summary>
    /// Value override of the ingredient
    /// </summary>
    public double? Value { get; set; }
}