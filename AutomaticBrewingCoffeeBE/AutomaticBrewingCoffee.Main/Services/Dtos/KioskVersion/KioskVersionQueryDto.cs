using Services.Base;

namespace Services.Dtos.KioskVersion;

public class KioskVersionQueryDto : BaseQuery
{
    public string? Status { get; set; }
}