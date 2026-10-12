using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IEmployeeAccountService
{
    Task<PaginatedResult<EmployeeAccountViewModel>> GetPagedAccountsAsync(PageRequest request);
    Task<EmployeeAccountViewModel> GetAccountByIdAsync(string id);
    Task<EmployeeAccountViewModel> GetAccountByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LookupItem>> GetEmployeesWithoutAccountAsync();
    Task<ResultMessage> UpsertAccountAsync(EmployeeAccountViewModel model);
}
