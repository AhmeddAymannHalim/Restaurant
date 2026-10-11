using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IExpenseService
{
    Task<PaginatedResult<ExpenseViewModel>> GetPagedExpensesAsync(PageRequest request);
    Task<ExpenseViewModel> GetExpenseByIdAsync(int id);
    Task<ResultMessage> UpsertExpenseAsync(ExpenseViewModel model);
    Task<ResultMessage> DeleteExpenseAsync(int id);
}
