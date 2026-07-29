using Services.Base;

namespace Services.Dtos.Dashboard.Total.Revenue;

public class TotalRevenueQueryDto : DateRangeQuery
{
    public string? OrganizationId { get; set; }

    public string? StoreId { get; set; }

    public string? KioskId { get; set; }
}