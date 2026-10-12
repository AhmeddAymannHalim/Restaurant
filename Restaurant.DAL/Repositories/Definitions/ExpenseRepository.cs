using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Expense>> GetPagedExpensesAsync(PageRequest request)
    {
        var query = _context.Expenses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search) || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Expense>.Create(data, total, request);
    }

    public async Task<Expense> GetExpenseByIdAsync(int id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertExpenseAsync(Expense entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.Expenses.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Expenses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                _context.Expenses.Update(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeleteExpenseAsync(int id)
    {
        try
        {
            var entity = await _context.Expenses.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.ExitExpenses.AnyAsync(a => a.ExpenseId == id))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteLinkedRecord"
                };
            }
            _context.Expenses.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
