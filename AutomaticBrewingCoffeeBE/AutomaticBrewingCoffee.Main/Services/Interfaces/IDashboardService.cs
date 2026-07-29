using Services.Base;
using Services.Dtos.Dashboard.Summary.Account;
using Services.Dtos.Dashboard.Summary.Kiosk;
using Services.Dtos.Dashboard.Summary.Order;
using Services.Dtos.Dashboard.Summary.Organization;
using Services.Dtos.Dashboard.Summary.Store;
using Services.Dtos.Dashboard.Total.Revenue;
using Services.Dtos.Dashboard.Traffic.HourlyPeak;
using Services.Dtos.Dashboard.Traffic.Order;

namespace Services.Interfaces;

public interface IDashboardService
{
    Task<BaseResult<OrderSummaryQueryDto, OrderSummaryDto>> GetOrderSummary(OrderSummaryQueryDto orderSummaryQueryDto);
    Task<BaseResult<KioskSummaryQueryDto, KioskSummaryDto>> GetKioskSummary(KioskSummaryQueryDto kioskSummaryQueryDto);

    Task<BaseResult<AccountSummaryQueryDto, AccountSummaryDto>> GetAccountSummary(
        AccountSummaryQueryDto accountSummaryDto);

    Task<BaseResult<OrganizationSummaryQueryDto, OrganizationSummaryDto>> GetOrganizationSummary(
        OrganizationSummaryQueryDto organizationSummaryDto);

    Task<BaseResult<StoreSummaryQueryDto, StoreSummaryDto>> GetStoreSummary(StoreSummaryQueryDto storeSummaryQuery);
    Task<BaseResult<TotalRevenueQueryDto, TotalRevenueDto>> GetTotalRevenue(TotalRevenueQueryDto totalRevenueQueryDto);
    Task<BaseResult<OrderTrafficQueryDto, OrderTrafficDto>> GetOrderTraffic(OrderTrafficQueryDto orderTrafficQueryDto);

    Task<BaseResult<HourlyPeakQueryDto, HourlyPeakDto>> GetHourlyPeak(HourlyPeakQueryDto query);
}