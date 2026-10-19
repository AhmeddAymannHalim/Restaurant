using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IAllowanceTypeService
{
    Task<PaginatedResult<AllowanceTypeViewModel>> GetPagedAllowanceTypesAsync(PageRequest request);
    Task<AllowanceTypeViewModel> GetAllowanceTypeByIdAsync(int id);
    Task<ResultMessage> UpsertAllowanceTypeAsync(AllowanceTypeViewModel model);
    Task<ResultMessage> DeleteAllowanceTypeAsync(int id);
    Task<string> NewAllowanceTypeCodeAsync();
}
