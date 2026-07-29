using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Store;
using Services.Interfaces;
using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Utils;

namespace Services.Implements
{
    public class StoreService : BaseService<StoreService>, IStoreService
    {
        public StoreService(
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

        public async Task<BaseResult<StoreQueryDto, Paginate<StoreDto>>> GetStores(StoreQueryDto storeQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetStores", storeQueryDto);

            var predicate = _unitOfWork.GetRepository<Store>()
                .BuildSearchPredicate(storeQueryDto.FilterQuery, storeQueryDto.FilterBy);

            Expression<Func<Store, bool>> isDeletedFilter = x => x.IsDeleted == false;
            predicate = ExpressionHelper.CombineExpressions<Store>(predicate, isDeletedFilter);

            var roles = GetAccountRolesFromJwt();

            if (roles[0].Equals(ERoleName.Organization.ToString()))
            {
                var referenceId = GetReferenceIdFromJwt();
                storeQueryDto.OrganizationId = referenceId;
            }

            if (storeQueryDto.StartDate is not null && storeQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Store>().BuildDateRangePredicate(
                    storeQueryDto.StartDate,
                    storeQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            if (storeQueryDto.Status != null)
            {
                Expression<Func<Store, bool>> statusFilter = x => x.Status == storeQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Store>(predicate, statusFilter);
            }

            if (storeQueryDto.OrganizationId != null)
            {
                Expression<Func<Store, bool>> statusFilter = x => x.OrganizationId == storeQueryDto.OrganizationId;
                predicate = ExpressionHelper.CombineExpressions<Store>(predicate, statusFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Store>()
                .BuildSortingQuery(storeQueryDto.SortBy, storeQueryDto.IsAsc);

            var stores = await _unitOfWork.GetRepository<Store>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: storeQueryDto.Page,
                size: storeQueryDto.Size,
                include: x => x.Include(x => x.Organization)
            );

            var storeDto = _mapper.Map<Paginate<StoreDto>>(stores);

            LogMessage(LogLevel.Information, "Out GetStores", storeDto);

            return new BaseResult<StoreQueryDto, Paginate<StoreDto>>
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Store>(),
                Request = storeQueryDto,
                Response = storeDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, StoreDto>> GetStore(string storeId)
        {
            LogMessage(LogLevel.Information, "In GetStore", storeId);

            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(
                    predicate: x => x.StoreId == storeId,
                    include: x => x.Include(x => x.Organization)
                );

            if (store == null)
            {
                return new BaseResult<string, StoreDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Store>(),
                    Request = storeId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var storeDto = _mapper.Map<StoreDto>(store);

            LogMessage(LogLevel.Information, "Out GetStore", storeDto);

            return new BaseResult<string, StoreDto>
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Store>(),
                Request = storeId,
                Response = storeDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<CreateStoreDto, StoreDto>> CreateStore(CreateStoreDto createStoreDto)
        {
            LogMessage(LogLevel.Information, "In CreateStore", createStoreDto);

            var newStore = _mapper.Map<Store>(createStoreDto);

            await _unitOfWork.GetRepository<Store>().InsertAsync(newStore);
            var result = await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Insert Store", result);

            var storeDto = _mapper.Map<StoreDto>(newStore);

            LogMessage(LogLevel.Information, "Out CreateStore", storeDto);

            return new BaseResult<CreateStoreDto, StoreDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Store>(),
                Request = createStoreDto,
                Response = storeDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<BaseResult<UpdateStoreDto, StoreDto>> UpdateStore(string storeId,
            UpdateStoreDto updateStoreDto)
        {
            var roles = GetAccountRolesFromJwt();
            if (roles?.Count > 0 && roles[0].Equals(ERoleName.Organization.ToString()))
            {
                var referenceId = GetReferenceIdFromJwt();
                if (referenceId is null)
                {
                    return new BaseResult<UpdateStoreDto, StoreDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Organization>(),
                        StatusCode = StatusCodes.Status404NotFound,
                        Request = updateStoreDto,
                        Response = null
                    };
                }

                if (updateStoreDto.OrganizationId != referenceId)
                {
                    return new BaseResult<UpdateStoreDto, StoreDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.Invalid<Organization>(),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Request = updateStoreDto,
                        Response = null
                    };
                }
            }

            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.StoreId == storeId);

            if (store == null)
            {
                return new BaseResult<UpdateStoreDto, StoreDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Store>(),
                    Request = updateStoreDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            store = _mapper.Map(updateStoreDto, store);

            _unitOfWork.GetRepository<Store>().Update(store);
            await _unitOfWork.CommitAsync();

            var storeDto = _mapper.Map<StoreDto>(store);

            return new BaseResult<UpdateStoreDto, StoreDto>
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Store>(),
                Request = updateStoreDto,
                Response = storeDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, StoreDto>> RemoveStore(string storeId)
        {
            LogMessage(LogLevel.Information, "In RemoveStore", storeId);

            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync(predicate: x => x.StoreId == storeId);

            if (store == null)
            {
                return new BaseResult<string, StoreDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Store>(),
                    Request = storeId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var kioskOfStore = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.StoreId == storeId
            );

            if (kioskOfStore is not null)
            {
                return new BaseResult<string, StoreDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyUsing<Store>(),
                    Request = storeId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            store.Delete();

            _unitOfWork.GetRepository<Store>().Update(store);
            await _unitOfWork.CommitAsync();

            var storeDto = _mapper.Map<StoreDto>(store);

            LogMessage(LogLevel.Information, "Out RemoveStore", storeDto);

            return new BaseResult<string, StoreDto>
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Store>(),
                Request = storeId,
                Response = storeDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}