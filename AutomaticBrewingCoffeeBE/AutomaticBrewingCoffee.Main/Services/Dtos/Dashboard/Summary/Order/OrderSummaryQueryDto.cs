using Services.Base;

namespace Services.Dtos.Dashboard.Summary.Order;

public class OrderSummaryQueryDto : DateRangeQuery
{
    public string? OrganizationId { get; set; }

    public string? StoreId { get; set; }

    public string? KioskId { get; set; }
}