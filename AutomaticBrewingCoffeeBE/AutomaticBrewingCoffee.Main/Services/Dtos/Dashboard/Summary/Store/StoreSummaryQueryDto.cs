using Services.Base;

namespace Services.Dtos.Dashboard.Summary.Store;

public class StoreSummaryQueryDto : DateRangeQuery
{
    public string? OrganizationId { get; set; }
}