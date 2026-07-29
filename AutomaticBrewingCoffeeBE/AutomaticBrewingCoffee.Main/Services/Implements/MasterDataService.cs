using System.ComponentModel.DataAnnotations;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using Services.Base;
using Services.Dtos.Metadata;
using Services.Interfaces;

namespace Services.Implements;

public class MasterDataService : IMasterDataService
{
    public BaseResult<string, List<EnumMetadataDto>> GetEnumMetadata<TEnum>() where TEnum : Enum
    {
        var data = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => new EnumMetadataDto
            {
                Value = Convert.ToInt32(e),
                Name = e.GetType()
                    .GetMember(e.ToString())
                    .FirstOrDefault()?
                    .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
            })
            .ToList();


        return new BaseResult<string, List<EnumMetadataDto>>()
        {
            StatusCode = StatusCodes.Status200OK,
            Message = $"{typeof(TEnum).Name} found",
            Request = typeof(TEnum).Name,
            IsSuccess = true,
            Response = data
        };
    }
}