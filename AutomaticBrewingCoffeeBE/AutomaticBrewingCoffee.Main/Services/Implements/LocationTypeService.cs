using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.LocationType;
using Services.Interfaces;
using System.Linq.Expressions;
using Services.Utils;

namespace Services.Implements
{
    public class LocationTypeService : BaseService<LocationTypeService>, ILocationTypeService
    {
        public LocationTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
        {
        }

        public async Task<BaseResult<LocationTypeQueryDto, Paginate<LocationTypeDto>>> GetLocationTypes(
            LocationTypeQueryDto locationTypeQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetLocationTypes", locationTypeQueryDto);

            var predicate = _unitOfWork.GetRepository<LocationType>()
                .BuildSearchPredicate(locationTypeQueryDto.FilterQuery, locationTypeQueryDto.FilterBy);

            Expression<Func<LocationType, bool>> isDeletedFilter = x => !x.IsDeleted;
            predicate = ExpressionHelper.CombineExpressions<LocationType>(predicate, isDeletedFilter);

            if (locationTypeQueryDto.StartDate is not null && locationTypeQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<LocationType>().BuildDateRangePredicate(
                    locationTypeQueryDto.StartDate,
                    locationTypeQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            var orderBy = _unitOfWork.GetRepository<LocationType>()
                .BuildSortingQuery(locationTypeQueryDto.SortBy, locationTypeQueryDto.IsAsc);

            var result = await _unitOfWork.GetRepository<LocationType>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: locationTypeQueryDto.Page,
                size: locationTypeQueryDto.Size
            );

            var resultDto = _mapper.Map<Paginate<LocationTypeDto>>(result);

            return new BaseResult<LocationTypeQueryDto, Paginate<LocationTypeDto>>
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<LocationType>(),
                Request = locationTypeQueryDto,
                Response = resultDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, LocationTypeDto>> GetLocationType(string locationTypeId)
        {
            var entity = await _unitOfWork.GetRepository<LocationType>()
                .SingleOrDefaultAsync(predicate: x => x.LocationTypeId == locationTypeId);

            if (entity == null)
            {
                return new BaseResult<string, LocationTypeDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<LocationType>(),
                    Request = locationTypeId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var dto = _mapper.Map<LocationTypeDto>(entity);

            return new BaseResult<string, LocationTypeDto>
            {
                IsSuccess = true,
                Message = MessageUtil.NotFound<LocationType>(),
                Request = locationTypeId,
                Response = dto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<CreateLocationTypeDto, LocationTypeDto>> CreateLocationType(
            CreateLocationTypeDto createLocationTypeDto)
        {
            var entity = _mapper.Map<LocationType>(createLocationTypeDto);

            await _unitOfWork.GetRepository<LocationType>().InsertAsync(entity);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<LocationTypeDto>(entity);

            return new BaseResult<CreateLocationTypeDto, LocationTypeDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<LocationType>(),
                Request = createLocationTypeDto,
                Response = resultDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<BaseResult<UpdateLocationTypeDto, LocationTypeDto>> UpdateLocationType(string locationTypeId,
            UpdateLocationTypeDto updateLocationTypeDto)
        {
            var entity = await _unitOfWork.GetRepository<LocationType>()
                .SingleOrDefaultAsync(predicate: x => x.LocationTypeId == locationTypeId);

            if (entity == null)
            {
                return new BaseResult<UpdateLocationTypeDto, LocationTypeDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<LocationType>(),
                    Request = updateLocationTypeDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            entity = _mapper.Map(updateLocationTypeDto, entity);

            _unitOfWork.GetRepository<LocationType>().Update(entity);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<LocationTypeDto>(entity);

            return new BaseResult<UpdateLocationTypeDto, LocationTypeDto>
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<LocationType>(),
                Request = updateLocationTypeDto,
                Response = resultDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, LocationTypeDto>> RemoveLocationType(string locationTypeId)
        {
            var entity = await _unitOfWork.GetRepository<LocationType>()
                .SingleOrDefaultAsync(predicate: x => x.LocationTypeId == locationTypeId);

            if (entity == null)
            {
                return new BaseResult<string, LocationTypeDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<LocationType>(),
                    Request = locationTypeId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var store = await _unitOfWork.GetRepository<Store>().SingleOrDefaultAsync(
                predicate: x => x.LocationTypeId == locationTypeId
            );

            if (store is not null)
            {
                return new BaseResult<string, LocationTypeDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyUsing<LocationType>(),
                    Request = locationTypeId,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            entity.Delete();

            _unitOfWork.GetRepository<LocationType>().Update(entity);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<LocationTypeDto>(entity);

            return new BaseResult<string, LocationTypeDto>
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<LocationType>(),
                Request = locationTypeId,
                Response = resultDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}