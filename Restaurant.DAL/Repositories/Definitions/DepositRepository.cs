using Shared.Helpers;
using Restaurant.DAL.IRepositories.Commons;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class DepositRepository : IDepositRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public DepositRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<Deposit>> GetPagedDepositsAsync(PageRequest request)
    {
        var query = _context.Deposits.AsNoTracking()
            .Include(a => a.User)
            .Where(a => !a.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ProcessCode.ToLower().Contains(search) || a.DeposerName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Deposit>.Create(data, total, request);
    }

    public async Task<Deposit> GetDepositByIdAsync(int id)
    {
        return await _context.Deposits.FindAsync(id);
    }

    public async Task<string> NewDepositCodeAsync()
    {
        var codes = await _context.Deposits.AsNoTracking().Select(a => a.ProcessCode).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertDepositAsync(Deposit entity, string userId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (entity.Id == 0)
            {
                entity.ProcessCode = await NewDepositCodeAsync();
                entity.UserId = userId;
                await _context.Deposits.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Deposits.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.ProcessCode = existing.ProcessCode;
                entity.CreatedTime = existing.CreatedTime;
                entity.IsDeleted = existing.IsDeleted;
                entity.UserId = existing.UserId;
                _context.Deposits.Update(entity);
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

    public async Task<ResultMessage> DeleteDepositAsync(int id)
    {
        try
        {
            var entity = await _context.Deposits.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Deposit, entity.Id);
            _context.Deposits.Remove(entity);
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
