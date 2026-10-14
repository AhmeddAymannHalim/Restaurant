using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentViewModel>> GetPagedDepartmentsAsync(PageRequest request);
    Task<DepartmentViewModel> GetDepartmentByIdAsync(int id);
    Task<ResultMessage> UpsertDepartmentAsync(DepartmentViewModel model);
    Task<ResultMessage> DeleteDepartmentAsync(int id);
}
