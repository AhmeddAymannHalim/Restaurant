using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISalaryService
{
    Task<PaginatedResult<SalaryViewModel>> GetPagedSalariesAsync(PageRequest request);
    Task<SalaryViewModel> GetSalaryByIdAsync(int id);
    Task<ResultMessage> UpsertSalaryAsync(SalaryViewModel model, string userId);
    Task<ResultMessage> DeleteSalaryAsync(int id);
    Task<string> NewSalaryCodeAsync();
    Task<EmployeeExpenseViewModel> GetEmployeeExpensesAsync(int employeeId, DateTime startDate, DateTime endDate);
}
