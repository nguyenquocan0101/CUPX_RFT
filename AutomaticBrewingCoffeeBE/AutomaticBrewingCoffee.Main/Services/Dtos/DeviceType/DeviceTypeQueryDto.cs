using Services.Base;

namespace Services.Dtos.DeviceType;

public class DeviceTypeQueryDto : BaseQuery
{
    public string? Status { get; set; }
    
    public bool IsMobileDevice { get; set; }
}