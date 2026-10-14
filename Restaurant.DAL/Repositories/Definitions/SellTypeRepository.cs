using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class SellTypeRepository : ISellTypeRepository
{
    private readonly ApplicationDbContext _context;

    public SellTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<SellType>> GetPagedSellTypesAsync(PageRequest request)
    {
        var query = _context.SellTypes.AsNoTracking().AsQueryable();

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

        return PaginatedResult<SellType>.Create(data, total, request);
    }

    public async Task<SellType> GetSellTypeByIdAsync(int id)
    {
        return await _context.SellTypes.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertSellTypeAsync(SellType entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                await _context.SellTypes.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.SellTypes.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.State = existing.State;
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedById = existing.CreatedById;
                entity.UpdatedAt = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(entity);
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

    public async Task<ResultMessage> DeleteSellTypeAsync(int id)
    {
        try
        {
            var entity = await _context.SellTypes.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.SellTypes.Remove(entity);
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
