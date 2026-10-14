using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly ApplicationDbContext _context;

    public CostCenterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<CostCenter>> GetPagedCostCentersAsync(PageRequest request)
    {
        var query = _context.CostCenters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search)
                || a.EnName.ToLower().Contains(search)
                || a.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<CostCenter>.Create(data, total, request);
    }

    public async Task<CostCenter> GetCostCenterByIdAsync(int id)
    {
        return await _context.CostCenters.FindAsync(id);
    }

    public async Task<string> NewCostCenterCodeAsync()
    {
        var codes = await _context.CostCenters.AsNoTracking().Select(a => a.Code).ToListAsync();

        var nextCode = codes
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return nextCode.ToString();
    }

    public async Task<ResultMessage> UpsertCostCenterAsync(CostCenter entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = await NewCostCenterCodeAsync();
                }

                entity.CreatedAt = DateTime.Now;
                await _context.CostCenters.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.CostCenters.FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.Code = string.IsNullOrWhiteSpace(entity.Code) ? existing.Code : entity.Code;
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
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeleteCostCenterAsync(int id)
    {
        try
        {
            var entity = await _context.CostCenters.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.CostCenters.Remove(entity);
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
