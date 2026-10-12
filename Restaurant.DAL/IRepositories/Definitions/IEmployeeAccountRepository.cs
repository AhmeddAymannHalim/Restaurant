using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IEmployeeAccountRepository
{
    Task<PaginatedResult<ApplicationUser>> GetPagedAccountsAsync(PageRequest request);
    Task<ApplicationUser> GetAccountByIdAsync(string id);
    Task<ApplicationUser> GetAccountByEmployeeIdAsync(int employeeId);
    Task<Employee> GetEmployeeByIdAsync(int employeeId);
    Task<List<Employee>> GetEmployeesWithoutAccountAsync();
    Task<ResultMessage> UpsertAccountAsync(ApplicationUser entity, int employeeId, string password);
}
