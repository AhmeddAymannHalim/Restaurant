using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISalaryTypeService
{
    Task<PaginatedResult<SalaryTypeViewModel>> GetPagedSalaryTypesAsync(PageRequest request);
    Task<SalaryTypeViewModel> GetSalaryTypeByIdAsync(int id);
    Task<ResultMessage> UpsertSalaryTypeAsync(SalaryTypeViewModel model);
    Task<ResultMessage> DeleteSalaryTypeAsync(int id);
}
