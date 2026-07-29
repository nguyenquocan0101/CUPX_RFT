using Services.Base;

namespace Services.Dtos.Dashboard.Traffic.HourlyPeak;

public class HourlyPeakQueryDto : DateRangeQuery
{
    public string? OrganizationId { get; set; }
    public string? StoreId { get; set; }
    public string? KioskId { get; set; }
    public string TimeZoneId { get; set; } = "SE Asia Standard Time";
    
    public int WindowStartHour { get; set; } = 0;
    
    public int WindowEndHour { get; set; } = 24;
}