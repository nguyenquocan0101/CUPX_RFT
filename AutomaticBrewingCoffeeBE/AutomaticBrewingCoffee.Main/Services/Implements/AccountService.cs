using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Account;
using Services.Dtos.Organization;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class AccountService : BaseService<IAccountService>, IAccountService
{
    public AccountService(
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

    public Task<Account?> GetFirebaseUserAsync(string firebaseId)
    {
        var user = _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == firebaseId);
        return user;
    }

    public async Task<BaseResult<AccountQueryDto, Paginate<AccountDto>>> GetAccounts(AccountQueryDto accountQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetAccounts");

        var predicate = _unitOfWork.GetRepository<Account>()
            .BuildSearchPredicate(accountQueryDto.FilterQuery, accountQueryDto.FilterBy);

        var adminAccountId = GetAccountIdFromJwt();

        if (!string.IsNullOrEmpty(adminAccountId))
        {
            Expression<Func<Account, bool>> statusFilter = x => x.AccountId != adminAccountId;
            predicate = ExpressionHelper.CombineExpressions<Account>(predicate, statusFilter);
        }


        if (accountQueryDto.Status != null)
        {
            Expression<Func<Account, bool>> statusFilter = x => x.Status == accountQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<Account>(predicate, statusFilter);
        }

        if (accountQueryDto.IsBanned != null)
        {
            Expression<Func<Account, bool>> statusFilter = x => x.IsBanned == accountQueryDto.IsBanned;
            predicate = ExpressionHelper.CombineExpressions<Account>(predicate, statusFilter);
        }

        if (accountQueryDto.StartDate is not null && accountQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<Account>().BuildDateRangePredicate(
                accountQueryDto.StartDate,
                accountQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions<Account>(predicate, dateRangePredicate);
        }

        var orderBy = _unitOfWork.GetRepository<Account>()
            .BuildSortingQuery(accountQueryDto.SortBy, accountQueryDto.IsAsc);

        var accounts = await _unitOfWork.GetRepository<Account>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: accountQueryDto.Page,
            size: accountQueryDto.Size
        );

        var accountDtos = _mapper.Map<Paginate<AccountDto>>(accounts);

        foreach (var accountDto in accountDtos.Items)
        {
            switch (accountDto.RoleName)
            {
                case nameof(ERoleName.Organization):
                {
                    var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
                        predicate: x => x.OrganizationId == accountDto.OrganizationId
                    );

                    var organizationDto = _mapper.Map<OrganizationDto>(organization);

                    accountDto.Organization = organizationDto;

                    break;
                }
            }
        }

        LogMessage(LogLevel.Information, "Out GetAccounts");

        return new BaseResult<AccountQueryDto, Paginate<AccountDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Store>(),
            Request = accountQueryDto,
            Response = accountDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, AccountDto>> GetAccount(string accountId)
    {
        LogMessage(LogLevel.Information, "In GetAccount");

        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId
        );

        if (account is null)
        {
            return new BaseResult<string, AccountDto>()
            {
                IsSuccess = false,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Account>(),
                Request = accountId
            };
        }

        var accountDto = _mapper.Map<AccountDto>(account);

        switch (account.RoleName)
        {
            case nameof(ERoleName.Organization):
            {
                var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
                    predicate: x => x.OrganizationId == account.OrganizationId
                );

                var organizationDto = _mapper.Map<OrganizationDto>(organization);

                accountDto.Organization = organizationDto;
                break;
            }
        }

        LogMessage(LogLevel.Information, "Out GetAccount");

        return new BaseResult<string, AccountDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Account>(),
            Request = accountId,
            StatusCode = StatusCodes.Status200OK,
            Response = accountDto
        };
    }
}