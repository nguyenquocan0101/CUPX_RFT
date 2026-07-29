using Domain.Enums;

namespace Services.Dtos.Sync;

public class MenuProductMappingSyncDto
{
    public string MenuId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    public BaseStatus Status { get; set; }
}