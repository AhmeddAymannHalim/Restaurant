using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IDeductionTypeService
{
    Task<PaginatedResult<DeductionTypeViewModel>> GetPagedDeductionTypesAsync(PageRequest request);
    Task<DeductionTypeViewModel> GetDeductionTypeByIdAsync(int id);
    Task<ResultMessage> UpsertDeductionTypeAsync(DeductionTypeViewModel model);
    Task<ResultMessage> DeleteDeductionTypeAsync(int id);
}
