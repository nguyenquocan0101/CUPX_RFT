namespace Services.Dtos.IngredientType;

public class IngredientTypeDto
{
    public string IngredientTypeId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;
}