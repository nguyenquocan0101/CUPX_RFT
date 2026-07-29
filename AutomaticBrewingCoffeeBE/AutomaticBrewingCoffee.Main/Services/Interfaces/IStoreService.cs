using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Store;

namespace Services.Interfaces
{
    public interface IStoreService
    {
        Task<BaseResult<StoreQueryDto, Paginate<StoreDto>>> GetStores(StoreQueryDto storeQueryDto);
        Task<BaseResult<string, StoreDto>> GetStore(string storeId);
        Task<BaseResult<CreateStoreDto, StoreDto>> CreateStore(CreateStoreDto createStoreDto);
        Task<BaseResult<UpdateStoreDto, StoreDto>> UpdateStore(string storeId, UpdateStoreDto updateStoreDto);
        Task<BaseResult<string, StoreDto>> RemoveStore(string storeId);
    }
}