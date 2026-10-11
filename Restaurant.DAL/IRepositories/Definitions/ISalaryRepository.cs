using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISalaryRepository
{
    Task<PaginatedResult<Salary>> GetPagedSalariesAsync(PageRequest request);
    Task<Salary> GetSalaryByIdAsync(int id);
    Task<ResultMessage> UpsertSalaryAsync(Salary entity, string userId);
    Task<ResultMessage> DeleteSalaryAsync(int id);
    Task<string> NewSalaryCodeAsync();
    Task<EmployeeExpenseViewModel> GetEmployeeExpensesAsync(int employeeId, DateTime startDate, DateTime endDate);
}
