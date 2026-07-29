using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.KioskType;

namespace Services.Interfaces;

public interface IKioskTypeService
{
    Task<BaseResult<KioskTypeQueryDto, Paginate<KioskTypeDto>>> GetKioskTypes(
        KioskTypeQueryDto kioskTypeQueryDto);

    Task<BaseResult<string, KioskTypeDto>> GetKioskType(string kioskTypeId);
    Task<BaseResult<CreateKioskTypeDto, KioskTypeDto>> CreateKioskType(CreateKioskTypeDto createKioskTypeDto);

    Task<BaseResult<UpdateKioskTypeDto, KioskTypeDto>> UpdateKioskType(string kioskTypeId,
        UpdateKioskTypeDto updateKioskTypeDto);

    Task<BaseResult<string, KioskTypeDto>> RemoveKioskType(string kioskTypeId);
}