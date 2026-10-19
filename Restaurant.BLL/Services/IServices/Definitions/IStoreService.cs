using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IStoreService
{
    Task<PaginatedResult<StoreViewModel>> GetPagedStoresAsync(PageRequest request);
    Task<StoreViewModel> GetStoreByIdAsync(int id);
    Task<ResultMessage> UpsertStoreAsync(StoreViewModel model);
    Task<ResultMessage> DeleteStoreAsync(int id);
    Task<string> NewStoreCodeAsync();
}
