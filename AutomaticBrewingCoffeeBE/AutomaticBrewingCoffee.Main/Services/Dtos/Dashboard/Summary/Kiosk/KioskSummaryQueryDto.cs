using Services.Base;

namespace Services.Dtos.Dashboard.Summary.Kiosk;

public class KioskSummaryQueryDto : DateRangeQuery
{
    public string? StoreId { get; set; }

    public string? OrganizationId { get; set; }
}