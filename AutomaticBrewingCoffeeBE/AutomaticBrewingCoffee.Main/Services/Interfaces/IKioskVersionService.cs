using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.KioskVersion;
using Services.Dtos.KioskVersionDeviceModel;
using Services.Dtos.KioskVersionProduct;

namespace Services.Interfaces;

public interface IKioskVersionService
{
    Task<BaseResult<KioskVersionQueryDto, Paginate<KioskVersionDto>>> GetKioskVersions(
        KioskVersionQueryDto kioskVersionQueryDto);

    Task<BaseResult<string, KioskVersionDto>> GetKioskVersion(string kioskVersionId);

    Task<BaseResult<CreateKioskVersionDto, KioskVersionDto>> CreateKioskVersion(
        CreateKioskVersionDto createKioskVersionDto);

    Task<BaseResult<UpdateKioskVersionDto, KioskVersionDto>> UpdateKioskVersion(string kioskVersionId,
        UpdateKioskVersionDto updateKioskVersionDto);

    Task<BaseResult<string, KioskVersionDto>> RemoveKioskVersion(string kioskVersionId);

    Task<BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>> AddKioskVersionDeviceModel(
        AddKioskVersionDeviceModelDto addKioskDeviceDto);

    Task<BaseResult<string, Paginate<KioskVersionDeviceModelDto>>> GetKioskVersionDeviceModels(
        string kioskVersionId, KioskVersionDeviceModelQueryDto kioskVersionDeviceModelQueryDto);

    Task<BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>> AddKioskVersionProduct(
        AddKioskVersionProductDto addKioskDeviceDto);

    Task<BaseResult<string, Paginate<KioskVersionProductDto>>> GetKioskVersionProduct(
        string kioskVersionId, KioskVersionProductQueryDto kioskVersionProductQueryDto);

    Task<BaseResult<string, Paginate<DeviceDto>>> GetValidDevices(string kioskVersionId, DeviceQueryDto deviceQueryDto);
}