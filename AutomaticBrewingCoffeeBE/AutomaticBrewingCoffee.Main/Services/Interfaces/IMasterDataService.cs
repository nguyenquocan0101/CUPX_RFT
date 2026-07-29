using Services.Base;
using Services.Dtos.Metadata;

namespace Services.Interfaces;

public interface IMasterDataService
{
    BaseResult<string, List<EnumMetadataDto>> GetEnumMetadata<TEnum>() where TEnum : Enum;
}