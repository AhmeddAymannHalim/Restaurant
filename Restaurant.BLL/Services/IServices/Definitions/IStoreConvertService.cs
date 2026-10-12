using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IStoreConvertService
{
    Task<PaginatedResult<StoreConvertViewModel>> GetPagedConvertsAsync(PageRequest request);
    Task<StoreConvertViewModel> GetConvertByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<ResultMessage> CreateConvertAsync(StoreConvertViewModel model);
    Task<ResultMessage> DeleteConvertAsync(int id);
}
