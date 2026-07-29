using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.IngredientType;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers;

[Route($"{ApiEndpointsConstant.API_ENDPOINT}/ingredient-types")]
[ApiController]
[TrimStrings]
public class IngredientTypesController : ControllerBase
{
    private readonly IIngredientTypeService _ingredientTypeService;

    public IngredientTypesController(IIngredientTypeService ingredientTypeService)
    {
        _ingredientTypeService = ingredientTypeService;
    }

    [HttpGet]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Get list of ingredient types",
        Description = "Retrieve a paginated list of ingredient types with optional filters."
    )]
    public async Task<ActionResult<BaseResult<IngredientTypeQueryDto, Paginate<IngredientTypeDto>>>> Get(
        [FromQuery] IngredientTypeQueryDto queryDto)
    {
        var response = await _ingredientTypeService.GetIngredientTypes(queryDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{ingredientTypeId}")]
    [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
    [SwaggerOperation(
        Summary = "Get ingredient type details",
        Description = "Retrieve detailed information about a specific ingredient type by its ID."
    )]
    public async Task<ActionResult<BaseResult<string, IngredientTypeDto>>> Get(string ingredientTypeId)
    {
        var response = await _ingredientTypeService.GetIngredientType(ingredientTypeId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Authorizes(nameof(ERoleName.Admin))]
    [SwaggerOperation(
        Summary = "Create a new ingredient type",
        Description = "Create a new ingredient type by providing necessary details."
    )]
    public async Task<ActionResult<BaseResult<CreateIngredientTypeDto, IngredientTypeDto>>> Post(
        [FromBody] CreateIngredientTypeDto createDto)
    {
        var response = await _ingredientTypeService.CreateIngredientType(createDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{ingredientTypeId}")]
    [Authorizes(nameof(ERoleName.Admin))]
    [SwaggerOperation(
        Summary = "Update ingredient type details",
        Description = "Update the details of an existing ingredient type by its ID."
    )]
    public async Task<ActionResult<BaseResult<UpdateIngredientTypeDto, IngredientTypeDto>>> Put(
        string ingredientTypeId,
        [FromBody] UpdateIngredientTypeDto updateDto)
    {
        var response = await _ingredientTypeService.UpdateIngredientType(ingredientTypeId, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{ingredientTypeId}")]
    [Authorizes(nameof(ERoleName.Admin))]
    [SwaggerOperation(
        Summary = "Delete an ingredient type",
        Description = "Soft delete an existing ingredient type by its ID."
    )]
    public async Task<ActionResult<BaseResult<string, IngredientTypeDto>>> Delete(string ingredientTypeId)
    {
        var response = await _ingredientTypeService.RemoveIngredientType(ingredientTypeId);
        return StatusCode(response.StatusCode, response);
    }
}