using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISellTypeService
{
    Task<PaginatedResult<SellTypeViewModel>> GetPagedSellTypesAsync(PageRequest request);
    Task<SellTypeViewModel> GetSellTypeByIdAsync(int id);
    Task<ResultMessage> UpsertSellTypeAsync(SellTypeViewModel model);
    Task<ResultMessage> DeleteSellTypeAsync(int id);
}
