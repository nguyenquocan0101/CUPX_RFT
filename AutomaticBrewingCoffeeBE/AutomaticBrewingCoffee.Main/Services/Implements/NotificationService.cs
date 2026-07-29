using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Notification;
using Services.Interfaces;
using Services.Utils;
using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Services.Implements;

public class NotificationService : BaseService<NotificationService>, INotificationService
{
    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<NotificationQueryDto, Paginate<NotificationDto>>> GetNotifications(
        NotificationQueryDto notificationQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetNotifications", notificationQueryDto);

        var repository = _unitOfWork.GetRepository<Notification>();

        var roles = GetAccountRolesFromJwt();

        var predicate = repository.BuildSearchPredicate(
            notificationQueryDto.FilterQuery, notificationQueryDto.FilterBy);

        if (roles[0].Equals(ERoleName.Organization.ToString()))
        {
            notificationQueryDto.AccountId = GetAccountIdFromJwt();
        }

        if (notificationQueryDto.StartDate is not null && notificationQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<Notification>().BuildDateRangePredicate(
                notificationQueryDto.StartDate,
                notificationQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(notificationQueryDto.AccountId))
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.NotificationRecipients.Any(r => r.AccountId == notificationQueryDto.AccountId);
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (!string.IsNullOrEmpty(notificationQueryDto.AccountRole))
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.NotificationRecipients.Any(r => r.AccountRole == notificationQueryDto.AccountRole);
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.IsRead is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.NotificationRecipients.Any(r => r.IsRead == notificationQueryDto.IsRead);
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.NotificationType is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.NotificationType == notificationQueryDto.NotificationType;
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.ReferenceType is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.ReferenceType == notificationQueryDto.ReferenceType;
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.ReferenceId is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.ReferenceId == notificationQueryDto.ReferenceId;
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.Severity is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.Severity == notificationQueryDto.Severity;
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        if (notificationQueryDto.CreatedBy is not null)
        {
            Expression<Func<Notification, bool>> receiverFilter =
                x => x.CreatedBy == notificationQueryDto.CreatedBy;
            predicate = ExpressionHelper.CombineExpressions(predicate, receiverFilter);
        }

        var orderBy = repository.BuildSortingQuery(notificationQueryDto.SortBy, notificationQueryDto.IsAsc);

        var notifications = await repository.GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: notificationQueryDto.Page,
            size: notificationQueryDto.Size,
            include: x =>
                x.Include(x => x.NotificationRecipients)
        );

        var dto = _mapper.Map<Paginate<NotificationDto>>(notifications,
            opts => { opts.Items["CurrentAccountId"] = GetAccountIdFromJwt(); });

        LogMessage(LogLevel.Information, "Out GetNotifications", dto);

        return new BaseResult<NotificationQueryDto, Paginate<NotificationDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Notification>(),
            Request = notificationQueryDto,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, NotificationDto>> GetNotification(string notificationId)
    {
        LogMessage(LogLevel.Information, "In GetNotification", notificationId);

        var entity = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(
            predicate: x => x.NotificationId == notificationId);

        if (entity == null)
        {
            return new BaseResult<string, NotificationDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Notification>(),
                Request = notificationId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var dto = _mapper.Map<NotificationDto>(entity,
            opts => { opts.Items["CurrentAccountId"] = GetAccountIdFromJwt(); });

        LogMessage(LogLevel.Information, "Out GetNotification", dto);

        return new BaseResult<string, NotificationDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Notification>(),
            Request = notificationId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<ReadNotificationDto, NotificationDto>> ReadNotification(
        ReadNotificationDto readNotificationDto)
    {
        var accountId = GetAccountIdFromJwt();

        var notificationRecipient = await _unitOfWork.GetRepository<NotificationRecipient>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId && x.NotificationId == readNotificationDto.NotificationId
        );

        if (notificationRecipient is null)
        {
            return new BaseResult<ReadNotificationDto, NotificationDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Notification>(),
                Request = readNotificationDto,
                Response = null,
            };
        }


        notificationRecipient.Read();

        _unitOfWork.GetRepository<NotificationRecipient>().Update(notificationRecipient);
        await _unitOfWork.CommitAsync();

        return new BaseResult<ReadNotificationDto, NotificationDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Notification>(),
            Request = readNotificationDto,
            Response = null,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<ReadNotificationsDto, List<NotificationDto>>> ReadNotifications(
        ReadNotificationsDto readNotificationsDto)
    {
        var accountId = GetAccountIdFromJwt();

        var notificationRecipients = await _unitOfWork.GetRepository<NotificationRecipient>().GetListAsync(
            predicate: x => x.AccountId == accountId && readNotificationsDto.NotificationIds.Contains(x.NotificationId)
        );

        if (notificationRecipients.IsNullOrEmpty())
        {
            return new BaseResult<ReadNotificationsDto, List<NotificationDto>>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Notification>(),
                Request = readNotificationsDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        foreach (var notificationRecipient in notificationRecipients)
        {
            notificationRecipient.Read();
        }

        _unitOfWork.GetRepository<NotificationRecipient>().UpdateRange(notificationRecipients);
        await _unitOfWork.CommitAsync();

        return new BaseResult<ReadNotificationsDto, List<NotificationDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Notification>(),
            Request = readNotificationsDto,
            Response = null,
            StatusCode = StatusCodes.Status200OK
        };
    }
}