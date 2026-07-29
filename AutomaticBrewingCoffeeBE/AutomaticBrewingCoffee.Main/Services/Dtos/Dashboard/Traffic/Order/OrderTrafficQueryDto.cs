using Services.Base;

namespace Services.Dtos.Dashboard.Traffic.Order;

public class OrderTrafficQueryDto : DateRangeQuery
{
    public string? OrganizationId { get; set; }

    public string? StoreId { get; set; }

    public string? KioskId { get; set; }

    public bool IncludeSunday { get; set; }

    public string TimeZoneId { get; set; } = "SE Asia Standard Time";
}