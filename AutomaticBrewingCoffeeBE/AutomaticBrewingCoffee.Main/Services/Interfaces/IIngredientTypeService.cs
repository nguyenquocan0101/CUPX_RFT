using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.IngredientType;

namespace Services.Interfaces;

public interface IIngredientTypeService
{
    Task<BaseResult<IngredientTypeQueryDto, Paginate<IngredientTypeDto>>> GetIngredientTypes(IngredientTypeQueryDto queryDto);
    Task<BaseResult<string, IngredientTypeDto>> GetIngredientType(string id);
    Task<BaseResult<CreateIngredientTypeDto, IngredientTypeDto>> CreateIngredientType(CreateIngredientTypeDto createDto);
    Task<BaseResult<UpdateIngredientTypeDto, IngredientTypeDto>> UpdateIngredientType(string id, UpdateIngredientTypeDto updateDto);
    Task<BaseResult<string, IngredientTypeDto>> RemoveIngredientType(string id);
}