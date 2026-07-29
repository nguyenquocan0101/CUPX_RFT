using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.LocationType;

namespace Services.Interfaces;

public interface ILocationTypeService
{
    Task<BaseResult<LocationTypeQueryDto, Paginate<LocationTypeDto>>> GetLocationTypes(
        LocationTypeQueryDto locationTypeQueryDto);

    Task<BaseResult<string, LocationTypeDto>> GetLocationType(string locationTypeId);

    Task<BaseResult<CreateLocationTypeDto, LocationTypeDto>> CreateLocationType(
        CreateLocationTypeDto createLocationTypeDto);

    Task<BaseResult<UpdateLocationTypeDto, LocationTypeDto>> UpdateLocationType(string locationTypeId,
        UpdateLocationTypeDto updateLocationTypeDto);

    Task<BaseResult<string, LocationTypeDto>> RemoveLocationType(string locationTypeId);
}