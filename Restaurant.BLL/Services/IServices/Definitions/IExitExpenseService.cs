using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IExitExpenseService
{
    Task<PaginatedResult<ExitExpenseViewModel>> GetPagedExitExpensesAsync(PageRequest request);
    Task<ExitExpenseViewModel> GetExitExpenseByIdAsync(int id);
    Task<ResultMessage> UpsertExitExpenseAsync(ExitExpenseViewModel model, string userId);
    Task<ResultMessage> DeleteExitExpenseAsync(int id);
}
