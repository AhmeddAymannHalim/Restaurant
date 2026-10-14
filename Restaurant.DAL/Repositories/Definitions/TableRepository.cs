using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class TableRepository : ITableRepository
{
    private readonly ApplicationDbContext _context;

    public TableRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Restaurant.DAL.Models.Table>> GetPagedTablesAsync(PageRequest request)
    {
        var query = _context.Tables.AsNoTracking().AsQueryable();

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

        return PaginatedResult<Restaurant.DAL.Models.Table>.Create(data, total, request);
    }

    public async Task<Restaurant.DAL.Models.Table> GetTableByIdAsync(int id)
    {
        return await _context.Tables.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertTableAsync(Restaurant.DAL.Models.Table entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.Tables.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.IsPending = existing.IsPending;
                _context.Tables.Update(entity);
                await _context.SaveChangesAsync();
            }

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

    public async Task<ResultMessage> DeleteTableAsync(int id)
    {
        try
        {
            var entity = await _context.Tables.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Tables.Remove(entity);
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
