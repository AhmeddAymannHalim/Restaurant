using Shared.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class TaxRepository : ITaxRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public TaxRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    public async Task<PaginatedResult<Tax>> GetPagedTaxesAsync(PageRequest request)
    {
        var query = _context.Taxes.AsNoTracking().AsQueryable();

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

        return PaginatedResult<Tax>.Create(data, total, request);
    }

    public async Task<Tax> GetTaxByIdAsync(int id)
    {
        return await _context.Taxes.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertTaxAsync(Tax entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
                await _context.Taxes.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Taxes.FirstOrDefaultAsync(a => a.Id == entity.Id);
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
                entity.UpdatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
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

    public async Task<ResultMessage> DeleteTaxAsync(int id)
    {
        try
        {
            var entity = await _context.Taxes.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.Taxes.Remove(entity);
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
