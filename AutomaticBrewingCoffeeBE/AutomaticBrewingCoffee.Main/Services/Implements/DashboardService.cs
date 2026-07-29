using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Dashboard.Summary.Account;
using Services.Dtos.Dashboard.Summary.Kiosk;
using Services.Dtos.Dashboard.Summary.Order;
using Services.Dtos.Dashboard.Summary.Organization;
using Services.Dtos.Dashboard.Summary.Store;
using Services.Dtos.Dashboard.Total.Revenue;
using Services.Dtos.Dashboard.Traffic.HourlyPeak;
using Services.Dtos.Dashboard.Traffic.Order;
using Services.Dtos.Order;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class DashboardService : BaseService<DashboardService>, IDashboardService
{
    public DashboardService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    ) : base(
        unitOfWork,
        mapper,
        loggerFactory,
        httpContextAccessor
    )
    {
    }

    private static TimeZoneInfo GetTz(string id)
        => TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(id) ? "SE Asia Standard Time" : id);

    public async Task<BaseResult<OrderSummaryQueryDto, OrderSummaryDto>> GetOrderSummary(
        OrderSummaryQueryDto orderSummaryQueryDto)
    {
        var roles = GetAccountRolesFromJwt();

        if (roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            orderSummaryQueryDto.OrganizationId = referenceId;
        }

        var predicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(
            orderSummaryQueryDto.StartDate,
            orderSummaryQueryDto.EndDate
        );

        if (orderSummaryQueryDto.OrganizationId is not null)
        {
            Expression<Func<Order, bool>>
                franchiseFilter = x => x.OrganizationId == orderSummaryQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, franchiseFilter);
        }

        if (orderSummaryQueryDto.StoreId is not null)
        {
            Expression<Func<Order, bool>>
                franchiseFilter = x => x.StoreId == orderSummaryQueryDto.StoreId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, franchiseFilter);
        }

        if (orderSummaryQueryDto.KioskId is not null)
        {
            Expression<Func<Order, bool>>
                franchiseFilter = x => x.KioskId == orderSummaryQueryDto.KioskId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, franchiseFilter);
        }

        var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: predicate);

        var recentOrders = orders.OrderByDescending(x => x.CreatedDate).Take(5).ToList();

        var result = new OrderSummaryDto
        {
            Total = orders.Count,
            Pending = orders.Count(o => o.Status == EOrderStatus.Pending.ToString()),
            Preparing = orders.Count(o => o.Status == EOrderStatus.Preparing.ToString()),
            Completed = orders.Count(o => o.Status == EOrderStatus.Completed.ToString()),
            Cancelled = orders.Count(o => o.Status == EOrderStatus.Cancelled.ToString()),
            Failed = orders.Count(o => o.Status == EOrderStatus.Failed.ToString()),
            RecentOrders = _mapper.Map<List<OrderInsideDto>>(recentOrders)
        };

        return new BaseResult<OrderSummaryQueryDto, OrderSummaryDto>()
        {
            IsSuccess = true,
            Request = orderSummaryQueryDto,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<Order>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<KioskSummaryQueryDto, KioskSummaryDto>> GetKioskSummary(
        KioskSummaryQueryDto kioskSummaryQueryDto)
    {
        var roles = GetAccountRolesFromJwt();

        if (roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            kioskSummaryQueryDto.OrganizationId = referenceId;
        }

        var predicate = _unitOfWork.GetRepository<Kiosk>().BuildDateRangePredicate(
            kioskSummaryQueryDto.StartDate,
            kioskSummaryQueryDto.EndDate
        );

        if (kioskSummaryQueryDto.OrganizationId is not null)
        {
            Expression<Func<Kiosk, bool>>
                franchiseFilter = x => x.Store!.OrganizationId == kioskSummaryQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate!, franchiseFilter);
        }

        if (kioskSummaryQueryDto.StoreId is not null)
        {
            Expression<Func<Kiosk, bool>>
                franchiseFilter = x => x.StoreId == kioskSummaryQueryDto.StoreId;
            predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate!, franchiseFilter);
        }

        var kiosks = await _unitOfWork.GetRepository<Kiosk>().GetListAsync(
            predicate: predicate,
            include: x => x.Include(x => x.Store)
        );

        var result = new KioskSummaryDto()
        {
            Total = kiosks.Count,
            Active = kiosks.Count(x => x.Status == EBaseStatus.Active.ToString()),
            Inactive = kiosks.Count(x => x.Status == EBaseStatus.Inactive.ToString()),
        };

        return new BaseResult<KioskSummaryQueryDto, KioskSummaryDto>()
        {
            IsSuccess = true,
            Request = kioskSummaryQueryDto,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<Kiosk>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<AccountSummaryQueryDto, AccountSummaryDto>> GetAccountSummary(
        AccountSummaryQueryDto accountSummaryQueryDto)
    {
        var predicate = _unitOfWork.GetRepository<Account>().BuildDateRangePredicate(
            accountSummaryQueryDto.StartDate,
            accountSummaryQueryDto.EndDate
        );

        var accounts = await _unitOfWork.GetRepository<Account>().GetListAsync(
            predicate: predicate
        );

        var result = new AccountSummaryDto()
        {
            Total = accounts.Count,
            Active = accounts.Count(x => x.Status == EBaseStatus.Active.ToString()),
            Inactive = accounts.Count(x => x.Status == EBaseStatus.Inactive.ToString())
        };

        return new BaseResult<AccountSummaryQueryDto, AccountSummaryDto>()
        {
            IsSuccess = true,
            Request = accountSummaryQueryDto,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<Kiosk>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<OrganizationSummaryQueryDto, OrganizationSummaryDto>> GetOrganizationSummary(
        OrganizationSummaryQueryDto organizationSummaryDto
    )
    {
        var predicate = _unitOfWork.GetRepository<Organization>().BuildDateRangePredicate(
            organizationSummaryDto.StartDate,
            organizationSummaryDto.EndDate
        );

        var organizations = await _unitOfWork.GetRepository<Organization>().GetListAsync(
            predicate: predicate
        );

        var result = new OrganizationSummaryDto()
        {
            Total = organizations.Count,
            Active = organizations.Count(x => x.Status == EBaseStatus.Active.ToString()),
            Inactive = organizations.Count(x => x.Status == EBaseStatus.Inactive.ToString())
        };

        return new BaseResult<OrganizationSummaryQueryDto, OrganizationSummaryDto>()
        {
            IsSuccess = true,
            Request = organizationSummaryDto,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<Organization>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<StoreSummaryQueryDto, StoreSummaryDto>> GetStoreSummary(
        StoreSummaryQueryDto storeSummaryQueryDto
    )
    {
        var roles = GetAccountRolesFromJwt();
        if (roles?.Count > 0 && roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            storeSummaryQueryDto.OrganizationId = referenceId;
        }

        var predicate = _unitOfWork.GetRepository<Store>().BuildDateRangePredicate(
            storeSummaryQueryDto.StartDate,
            storeSummaryQueryDto.EndDate
        );

        if (storeSummaryQueryDto.OrganizationId is not null)
        {
            Expression<Func<Store, bool>>
                franchiseFilter = x => x.OrganizationId == storeSummaryQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions<Store>(predicate!, franchiseFilter);
        }

        var organizations = await _unitOfWork.GetRepository<Store>().GetListAsync(
            predicate: predicate
        );


        var result = new StoreSummaryDto()
        {
            Total = organizations.Count,
            Active = organizations.Count(x => x.Status == EBaseStatus.Active.ToString()),
            Inactive = organizations.Count(x => x.Status == EBaseStatus.Inactive.ToString())
        };

        return new BaseResult<StoreSummaryQueryDto, StoreSummaryDto>()
        {
            IsSuccess = true,
            Request = storeSummaryQueryDto,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<Store>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<TotalRevenueQueryDto, TotalRevenueDto>> GetTotalRevenue(
        TotalRevenueQueryDto totalRevenueQueryDto)
    {
        var roles = GetAccountRolesFromJwt();
        if (roles?.Count > 0 && roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            totalRevenueQueryDto.OrganizationId = referenceId;
        }

        var predicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(
            totalRevenueQueryDto.StartDate, // giả sử UTC
            totalRevenueQueryDto.EndDate // [Start, End) hoặc [Start, End] tuỳ hàm của bạn
        );

        if (totalRevenueQueryDto.OrganizationId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.OrganizationId == totalRevenueQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, f);
        }

        if (totalRevenueQueryDto.StoreId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.StoreId == totalRevenueQueryDto.StoreId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, f);
        }

        if (totalRevenueQueryDto.KioskId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.KioskId == totalRevenueQueryDto.KioskId;
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, f);
        }


        {
            Expression<Func<Order, bool>> f = x => x.Status == EOrderStatus.Completed.ToString();
            predicate = ExpressionHelper.CombineExpressions<Order>(predicate!, f);
        }


        var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: predicate);
        var currentRevenue = orders.Sum(o => (decimal?)(o.FinalAmount ?? 0m)) ?? 0m;

        var period = (totalRevenueQueryDto.EndDate - totalRevenueQueryDto.StartDate);
        var prevStart = totalRevenueQueryDto.StartDate - period;
        var prevEnd = totalRevenueQueryDto.StartDate;


        var prevPredicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(prevStart, prevEnd);


        if (totalRevenueQueryDto.OrganizationId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.OrganizationId == totalRevenueQueryDto.OrganizationId;
            prevPredicate = ExpressionHelper.CombineExpressions<Order>(prevPredicate!, f);
        }

        if (totalRevenueQueryDto.StoreId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.StoreId == totalRevenueQueryDto.StoreId;
            prevPredicate = ExpressionHelper.CombineExpressions<Order>(prevPredicate!, f);
        }

        if (totalRevenueQueryDto.KioskId is not null)
        {
            Expression<Func<Order, bool>> f = x => x.KioskId == totalRevenueQueryDto.KioskId;
            prevPredicate = ExpressionHelper.CombineExpressions<Order>(prevPredicate!, f);
        }


        {
            Expression<Func<Order, bool>> f = x => x.Status == EOrderStatus.Completed.ToString();
            prevPredicate = ExpressionHelper.CombineExpressions<Order>(prevPredicate!, f);
        }


        var prevOrders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: prevPredicate);
        var prevRevenue = prevOrders.Sum(o => (decimal?)(o.FinalAmount ?? 0m)) ?? 0m;


        double growthRate = 0d;
        if (prevRevenue > 0m)
        {
            growthRate = Math.Round((double)((currentRevenue - prevRevenue) / prevRevenue * 100m), 2);
        }

        var result = new TotalRevenueDto
        {
            Revenue = Math.Round(currentRevenue, 2),
            GrowthRatePercent = growthRate
        };

        return new BaseResult<TotalRevenueQueryDto, TotalRevenueDto>()
        {
            IsSuccess = true,
            Request = totalRevenueQueryDto,
            Message = MessageUtil.SummarizeSuccess<Order>(),
            Response = result,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<OrderTrafficQueryDto, OrderTrafficDto>> GetOrderTraffic(
        OrderTrafficQueryDto orderTrafficQueryDto)
    {
        var roles = GetAccountRolesFromJwt();
        if (roles?.Count > 0 && roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            orderTrafficQueryDto.OrganizationId = referenceId;
        }

        var predicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(
            orderTrafficQueryDto.StartDate,
            orderTrafficQueryDto.EndDate
        );

        if (orderTrafficQueryDto.OrganizationId is not null)
            predicate = ExpressionHelper.CombineExpressions(predicate,
                x => x.OrganizationId == orderTrafficQueryDto.OrganizationId);

        if (orderTrafficQueryDto.StoreId is not null)
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.StoreId == orderTrafficQueryDto.StoreId);

        if (orderTrafficQueryDto.KioskId is not null)
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.KioskId == orderTrafficQueryDto.KioskId);

        predicate = ExpressionHelper.CombineExpressions(predicate, x => x.Status == EOrderStatus.Completed.ToString());

        var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(predicate: predicate);

        var vnTimeZone = GetTz(orderTrafficQueryDto.TimeZoneId);

        var trafficByShift = orders
            .Select(o =>
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedDate, vnTimeZone);
                var dow = (int)localTime.DayOfWeek == 0 ? 7 : (int)localTime.DayOfWeek;
                var shift = localTime.TimeOfDay >= TimeSpan.FromHours(6) && localTime.TimeOfDay < TimeSpan.FromHours(18)
                    ? "day"
                    : "night";
                return new { Dow = dow, Shift = shift };
            })
            .Where(x => orderTrafficQueryDto.IncludeSunday || x.Dow is >= 1 and <= 6)
            .GroupBy(x => new { x.Dow, x.Shift })
            .Select(g => new OrderTrafficByShiftDto
            {
                Dow = g.Key.Dow,
                DowLabel = g.Key.Dow switch
                {
                    1 => "Hai",
                    2 => "Ba",
                    3 => "Tư",
                    4 => "Năm",
                    5 => "Sáu",
                    6 => "Bảy",
                    7 => "Chủ nhật",
                    _ => "?"
                },
                Shift = g.Key.Shift,
                Count = g.Count()
            })
            .OrderBy(x => x.Dow)
            .ThenBy(x => x.Shift == "day" ? 0 : 1)
            .ToList();

        var windowDays = 0;
        var currentPeriodTotal = 0;
        var previousPeriodTotal = 0;

        if (orderTrafficQueryDto.StartDate is not null && orderTrafficQueryDto.EndDate is not null)
        {
            var currStart = orderTrafficQueryDto.StartDate; // inclusive
            var currEnd = orderTrafficQueryDto.EndDate!.Value; // exclusive
            var period = currEnd - currStart; // TimeSpan, giữ luôn phần giờ/phút/giây
            windowDays = period.Value.Days;
            var prevStart = currStart - period; // inclusive
            var prevEnd = currStart; // exclusive

            Expression<Func<Order, bool>> basePred = x => x.Status == EOrderStatus.Completed.ToString();

            if (orderTrafficQueryDto.OrganizationId is not null)
                basePred = ExpressionHelper.CombineExpressions(basePred,
                    x => x.OrganizationId == orderTrafficQueryDto.OrganizationId);
            if (orderTrafficQueryDto.StoreId is not null)
                basePred = ExpressionHelper.CombineExpressions(basePred,
                    x => x.StoreId == orderTrafficQueryDto.StoreId);
            if (orderTrafficQueryDto.KioskId is not null)
                basePred = ExpressionHelper.CombineExpressions(basePred,
                    x => x.KioskId == orderTrafficQueryDto.KioskId);

            var currentPred = ExpressionHelper.CombineExpressions(
                basePred,
                _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(currStart, currEnd)!
            );

            var previousPred = ExpressionHelper.CombineExpressions(
                basePred,
                _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(prevStart, prevEnd)!
            );

            var repo = _unitOfWork.GetRepository<Order>();
            currentPeriodTotal = (await repo.GetListAsync(selector: _ => 1, predicate: currentPred)).Count;
            previousPeriodTotal = (await repo.GetListAsync(selector: _ => 1, predicate: previousPred)).Count;
        }

        var result = new OrderTrafficDto
        {
            WindowDays = windowDays,
            TrafficByShift = trafficByShift,
            TotalCurrentPeriod = currentPeriodTotal,
            TotalPreviousPeriod = previousPeriodTotal
        };

        return new BaseResult<OrderTrafficQueryDto, OrderTrafficDto>
        {
            IsSuccess = true,
            Response = result,
            Request = orderTrafficQueryDto,
            Message = MessageUtil.SummarizeSuccess<Order>(),
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<HourlyPeakQueryDto, HourlyPeakDto>> GetHourlyPeak(HourlyPeakQueryDto query)
    {
        var tz = GetTz(query.TimeZoneId);
        var repo = _unitOfWork.GetRepository<Order>();

        var roles = GetAccountRolesFromJwt();
        if (roles?.Count > 0 && roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            query.OrganizationId = referenceId;
        }

        // Base predicate: [Start, End)
        var predicate = repo.BuildDateRangePredicate(query.StartDate, query.EndDate);

        // Filters
        if (!string.IsNullOrEmpty(query.OrganizationId))
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.OrganizationId == query.OrganizationId);

        if (!string.IsNullOrEmpty(query.StoreId))
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.StoreId == query.StoreId);

        if (!string.IsNullOrEmpty(query.KioskId))
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.KioskId == query.KioskId);

        // Chỉ lấy đơn Completed
        predicate = ExpressionHelper.CombineExpressions(predicate, x => x.Status == EOrderStatus.Completed.ToString());

        // Lấy đúng cột cần
        var rows = await repo.GetListAsync(
            selector: o => new
            {
                o.CreatedDate,
                Amount = (decimal?)(o.FinalAmount ?? 0m) ?? 0m
            },
            predicate: predicate
        );

        // Buckets 0..23 cho amount & count
        var start = Math.Clamp(query.WindowStartHour, 0, 23);
        var end = Math.Clamp(query.WindowEndHour, 0, 23);

        if (end < start)
            throw new ArgumentException("WindowEndHour must be >= WindowStartHour.");

        var hours = Enumerable.Range(start, end - start + 1).ToArray();

        var amountBuckets = hours.ToDictionary(h => h, _ => 0m);
        var countBuckets = hours.ToDictionary(h => h, _ => 0);

        foreach (var r in rows)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(r.CreatedDate, tz);
            var h = local.Hour;
            if (h >= start && h <= end) // chỉ cộng khi nằm trong cửa sổ
            {
                amountBuckets[h] += r.Amount;
                countBuckets[h] += 1;
            }
        }

        var points = amountBuckets
            .OrderBy(kv => kv.Key)
            .Select(kv =>
            {
                var hour = kv.Key;
                var amount = kv.Value;
                var cnt = countBuckets[hour];
                return new HourlyPointDto
                {
                    Hour = $"{hour:00}:00",
                    IsPeak = false,
                    TotalAmount = amount,
                    OrderCount = cnt
                };
            })
            .ToList();

        // Peak theo doanh số (có thể đổi sang theo số đơn nếu cần)
        var peak = points.OrderByDescending(p => p.TotalAmount).FirstOrDefault();
        if (peak is not null)
        {
            var idx = points.FindIndex(p => p.Hour == peak.Hour);
            points[idx] = new HourlyPointDto
            {
                Hour = peak.Hour,
                IsPeak = true,
                TotalAmount = peak.TotalAmount,
                OrderCount = peak.OrderCount
            };
            peak = points[idx];
        }

        var windowDays = 0;

        if (query.StartDate is not null && query.EndDate is not null)
        {
            windowDays = query.EndDate.Value.Day - query.StartDate.Value.Day;
        }

        var result = new HourlyPeakDto
        {
            WindowDays = windowDays,
            Peak = peak,
            Points = points,
            WindowStartHour = query.WindowStartHour,
            WindowEndHour = query.WindowEndHour,
        };

        return new BaseResult<HourlyPeakQueryDto, HourlyPeakDto>()
        {
            IsSuccess = true,
            Request = query,
            Response = result,
            Message = MessageUtil.SummarizeSuccess<HourlyPeakDto>(),
            StatusCode = StatusCodes.Status200OK
        };
    }
}