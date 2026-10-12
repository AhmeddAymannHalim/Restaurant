using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class StoreRepository : IStoreRepository
{
    private readonly ApplicationDbContext _context;

    public StoreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Store>> GetPagedStoresAsync(PageRequest request)
    {
        var query = _context.Stores.AsNoTracking().Include(a => a.Storekeeper).AsQueryable();

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

        return PaginatedResult<Store>.Create(data, total, request);
    }

    public async Task<Store> GetStoreByIdAsync(int id)
    {
        return await _context.Stores.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertStoreAsync(Store entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                entity.StoreProducts = await _context.Products.AsNoTracking()
                    .Select(a => new StoreProduct { ProductId = a.Id })
                    .ToListAsync();
                await _context.Stores.AddAsync(entity);
            }
            else
            {
                if (!await _context.Stores.AnyAsync(a => a.Id == entity.Id))
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                _context.Stores.Update(entity);
            }

            await _context.SaveChangesAsync();

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

    public async Task<ResultMessage> DeleteStoreAsync(int id)
    {
        try
        {
            var entity = await _context.Stores.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.StoreProducts.AnyAsync(a => a.StoreId == id && a.Quantity > 0))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteHasStock"
                };
            }

            _context.Stores.Remove(entity);
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
