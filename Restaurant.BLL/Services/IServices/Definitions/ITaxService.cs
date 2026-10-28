using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ITaxService
{
    Task<PaginatedResult<TaxViewModel>> GetPagedTaxesAsync(PageRequest request);
    Task<TaxViewModel> GetTaxByIdAsync(int id);
    Task<ResultMessage> UpsertTaxAsync(TaxViewModel model);
    Task<ResultMessage> DeleteTaxAsync(int id);
}
