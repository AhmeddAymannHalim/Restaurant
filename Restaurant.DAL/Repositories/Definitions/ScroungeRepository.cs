using Restaurant.DAL.IRepositories.Commons;
using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ScroungeRepository : IScroungeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public ScroungeRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<Scrounge>> GetPagedScroungesAsync(PageRequest request)
    {
        var query = _context.Scrounges.AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Employee.ArName.ToLower().Contains(search)
                || a.Employee.EnName.ToLower().Contains(search)
                || a.Info.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Scrounge>.Create(data, total, request);
    }

    public async Task<Scrounge> GetScroungeByIdAsync(int id)
    {
        return await _context.Scrounges.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertScroungeAsync(Scrounge entity, string userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (entity.Id == 0)
            {
                entity.UserId = userId;
                await _context.Scrounges.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Scrounges.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
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
                _context.Scrounges.Update(entity);
                await _context.SaveChangesAsync();
            }

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Scrounge, entity.Id);

            var debitCode = entity.State == MoneyState.Incentive ? AccountCodes.Salaries
                : entity.State == MoneyState.Dept ? AccountCodes.EmployeesPayable
                : null;
            if (debitCode is not null)
            {
                var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Scrounge,
                    entity.Id,
                    entity.Date,
                    $"{entity.State} {entity.Id}",
                    new[]
                    {
                        new JournalLineRequest(debitCode, entity.Amount, 0),
                        new JournalLineRequest(AccountCodes.Cash, 0, entity.Amount)
                    });
                if (!posting.Success)
                {
                    return posting;
                }
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

    public async Task<ResultMessage> DeleteScroungeAsync(int id)
    {
        try
        {
            var entity = await _context.Scrounges.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Scrounge, entity.Id);
            _context.Scrounges.Remove(entity);
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
