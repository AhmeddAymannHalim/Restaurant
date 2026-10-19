using Shared.Helpers;
using Restaurant.DAL.IRepositories.Commons;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ExitExpenseRepository : IExitExpenseRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public ExitExpenseRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<ExitExpense>> GetPagedExitExpensesAsync(PageRequest request)
    {
        var query = _context.ExitExpenses.AsNoTracking()
            .Include(a => a.Expense)
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Expense.ArName.ToLower().Contains(search) || a.Expense.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<ExitExpense>.Create(data, total, request);
    }

    public async Task<ExitExpense> GetExitExpenseByIdAsync(int id)
    {
        return await _context.ExitExpenses.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertExitExpenseAsync(ExitExpense entity, string userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (entity.Id == 0)
            {
                entity.UserId = userId;
                entity.Date = DateTime.Now;
                await _context.ExitExpenses.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.ExitExpenses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.Date = existing.Date;
                entity.UserId = existing.UserId;
                _context.ExitExpenses.Update(entity);
                await _context.SaveChangesAsync();
            }


            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteExitExpenseAsync(int id)
    {
        try
        {
            var entity = await _context.ExitExpenses.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.ExitExpense, entity.Id);
            _context.ExitExpenses.Remove(entity);
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
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
