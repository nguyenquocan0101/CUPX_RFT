using Services.Dtos.KioskVersion;
using Services.Dtos.Product;

namespace Services.Dtos.KioskVersionProduct;

public class KioskVersionProductInsideDto
{
    public string KioskVersionId { get; set; } = null!;

    public virtual KioskVersionInsideDto KioskVersion { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public virtual ProductDto Product { get; set; } = null!;
}