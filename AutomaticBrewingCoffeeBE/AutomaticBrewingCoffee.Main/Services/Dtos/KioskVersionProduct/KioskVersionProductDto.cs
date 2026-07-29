using Services.Dtos.KioskVersion;
using Services.Dtos.Product;

namespace Services.Dtos.KioskVersionProduct;

public class KioskVersionProductDto
{
    public string KioskVersionId { get; set; } = null!;

    public virtual KioskVersionDto? KioskVersion { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public virtual ProductDto? Product { get; set; } = null!;
}