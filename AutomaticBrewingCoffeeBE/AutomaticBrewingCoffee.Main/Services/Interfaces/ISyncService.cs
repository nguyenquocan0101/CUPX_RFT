using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Sync;
using Services.Dtos.SyncEvent;
using Services.Dtos.SyncTask;

namespace Services.Interfaces;

public interface ISyncService
{
    Task<BaseResult<string, SynchronizedKioskDataDto>> SynchronizedKioskData(string kioskId);
    Task<BaseResult<string, OverridenKioskDataDto>> OverridenKioskData(string kioskId);

    Task<BaseResult<SyncTaskQueryDto, Paginate<SyncTaskDto>>> GetSyncTasks(SyncTaskQueryDto syncTaskQueryDto);
    Task<BaseResult<SyncEventQueryDto, Paginate<SyncEventDto>>> GetSyncEvents(SyncEventQueryDto syncEventQueryDto);
}