using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IStoreRepository
{
    Task<PaginatedResult<Store>> GetPagedStoresAsync(PageRequest request);
    Task<Store> GetStoreByIdAsync(int id);
    Task<ResultMessage> UpsertStoreAsync(Store entity);
    Task<ResultMessage> DeleteStoreAsync(int id);
    Task<string> NewStoreCodeAsync();
}
