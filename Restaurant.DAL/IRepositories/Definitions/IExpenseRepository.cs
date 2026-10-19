using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IExpenseRepository
{
    Task<PaginatedResult<Expense>> GetPagedExpensesAsync(PageRequest request);
    Task<Expense> GetExpenseByIdAsync(int id);
    Task<ResultMessage> UpsertExpenseAsync(Expense entity);
    Task<string> NewExpenseCodeAsync();
    Task<ResultMessage> DeleteExpenseAsync(int id);
}
