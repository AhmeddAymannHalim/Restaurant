using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IExitExpenseRepository
{
    Task<PaginatedResult<ExitExpense>> GetPagedExitExpensesAsync(PageRequest request);
    Task<ExitExpense> GetExitExpenseByIdAsync(int id);
    Task<ResultMessage> UpsertExitExpenseAsync(ExitExpense entity, string userId);
    Task<ResultMessage> DeleteExitExpenseAsync(int id);
}
