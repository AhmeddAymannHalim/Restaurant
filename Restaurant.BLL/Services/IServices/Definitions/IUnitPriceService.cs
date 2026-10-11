using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IUnitPriceService
{
    Task<PaginatedResult<UnitPriceViewModel>> GetPagedUnitPricesAsync(PageRequest request);
    Task<UnitPriceViewModel> GetUnitPriceByIdAsync(int id);
    Task<ResultMessage> UpsertUnitPriceAsync(UnitPriceViewModel model);
    Task<ResultMessage> DeleteUnitPriceAsync(int id);
}
