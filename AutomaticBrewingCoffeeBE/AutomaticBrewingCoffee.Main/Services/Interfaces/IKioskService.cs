using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.Kiosk;
using Services.Dtos.KioskDevice;

namespace Services.Interfaces
{
    public interface IKioskService
    {
        Task<BaseResult<KioskQueryDto, Paginate<KioskDto>>> GetKiosks(KioskQueryDto kioskQueryDto);
        Task<BaseResult<KioskQueryDto, Paginate<KioskDto>>> GetNoMenuKiosks(KioskQueryDto kioskQueryDto);
        Task<BaseResult<string, KioskDto>> GetKiosk(string kioskId);
        Task<BaseResult<string, KioskDto>> GetCurrentKiosk();
        Task<BaseResult<CreateKioskDto, KioskDto>> CreateKiosk(CreateKioskDto createKioskDto);
        Task<BaseResult<UpdateKioskDto, KioskDto>> UpdateKiosk(string kioskId, UpdateKioskDto updateKioskDto);
        Task<BaseResult<string, KioskDto>> RemoveKiosk(string kioskId);
        Task<BaseResult<AddKioskDeviceDto, KioskDeviceDto>> AddKioskDevice(AddKioskDeviceDto addKioskDeviceDto);

        Task<BaseResult<string, KioskDeviceDto>> ChangeKioskDeviceStatus(string kioskDeviceId,
            ChangeKioskDeviceStatusDto changeKioskDeviceStatusDto);

        Task<BaseResult<string, KioskDeviceDto>> DisposeKioskDevice(string kioskDeviceId);

        Task<BaseResult<ReplaceDeviceDto, KioskDeviceDto>> ReplaceDevice(string kioskDeviceId,
            ReplaceDeviceDto replaceDeviceDto);

        Task<BaseResult<string, KioskDeviceOnHubDto>> GetKioskDeviceOnHub(string kioskDeviceId);

        Task<BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>> GetKioskDeviceOnPlace(string kioskId,
            KioskDeviceOnPlaceQueryDto kioskDeviceOnPlaceQueryDto);

        Task<MemoryStream?> ExportKioskSetup(string kioskId);

        Task<BaseResult<AssignKioskMenuDto, KioskDto>> AssignKioskMenu(AssignKioskMenuDto assignKioskMenuDto);
        Task RemoveAllKioskDeviceOnHub();
        
        Task<BaseResult<string, KioskDto>> Clean();
        
        Task<BaseResult<string, KioskDto>> Ping();
    }
}