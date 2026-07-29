using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.DeviceFunction;
using Services.Dtos.DeviceIngredient;
using Services.Validations;

namespace Services.Dtos.DeviceModel;

public class CreateDeviceModelDto
{
    [StringLength(300)] public string? ModelName { get; set; } = null!;

    [StringLength(300)] public string? Manufacturer { get; set; } = null!;

    [StringLength(50)] public string? DeviceTypeId { get; set; }

    [MatchEnum(typeof(EBaseStatus))] public string Status { get; set; } = null!;

    public List<DeviceFunctionNestedDto>? DeviceFunctions { get; set; }

    public List<DeviceIngredientNestedDto>? DeviceIngredients { get; set; }
}