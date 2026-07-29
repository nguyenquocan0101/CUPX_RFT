namespace Services.Dtos.Sync;

public class MenuProductMappingSyncDto
{
    public string MenuId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    public string Status { get; set; } = null!;
}