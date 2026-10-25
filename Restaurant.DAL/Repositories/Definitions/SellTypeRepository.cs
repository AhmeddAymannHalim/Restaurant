using Shared.Helpers;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContext;

    public SellTypeRepository(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
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
            // the increase only means something for a price that follows the last purchase
            if (!entity.IsLastPrice)
            {
                entity.PlusPrice = null;
            }

            if (entity.Id == 0)
            {
                entity.CreatedAt = DateTime.Now;
                entity.CreatedById = _httpContext.HttpContext.User.Identity.Name ?? "";
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

    public async Task<ResultMessage> DeleteSellTypeAsync(int id)
    {
        try
        {
            var entity = await _context.SellTypes.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            // item prices are stored in the column picked by the sell type id (1..6): a type whose column holds prices must stay
            var hasPrices = false;
            switch (entity.Id)
            {
                case 1: hasPrices = await _context.ItemUnits.AnyAsync(a => a.Whole != null); break;
                case 2: hasPrices = await _context.ItemUnits.AnyAsync(a => a.HalfWhole != null); break;
                case 3: hasPrices = await _context.ItemUnits.AnyAsync(a => a.Retail != null); break;
                case 4: hasPrices = await _context.ItemUnits.AnyAsync(a => a.EndUsers != null); break;
                case 5: hasPrices = await _context.ItemUnits.AnyAsync(a => a.WeightedAverage != null); break;
                case 6: hasPrices = await _context.ItemUnits.AnyAsync(a => a.Export != null); break;
            }

            if (hasPrices)
            {
                return new ResultMessage { Success = false, Message = "CannotDeleteErpPriceType" };
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
