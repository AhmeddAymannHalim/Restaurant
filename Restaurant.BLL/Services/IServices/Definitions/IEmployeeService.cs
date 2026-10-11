using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeViewModel>> GetPagedEmployeesAsync(PageRequest request);
    Task<EmployeeViewModel> GetEmployeeByIdAsync(int id);
    Task<string> NewEmployeeCodeAsync();
    Task<ResultMessage> UpsertEmployeeAsync(EmployeeViewModel model);
    Task<ResultMessage> DeleteEmployeeAsync(int id);
}
