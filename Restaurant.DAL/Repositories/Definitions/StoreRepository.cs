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
        var query = _context.Stores.AsNoTracking().Include(a => a.Employee).AsQueryable();

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
        return await _context.Stores
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async Task<string> NewStoreCodeAsync()
    {
        var codes = await _context.Stores.AsNoTracking().Select(a => a.Code).ToListAsync();

        return (codes.Where(code => int.TryParse(code, out _)).Select(int.Parse).DefaultIfEmpty(0).Max() + 1).ToString();
    }

    public async Task<ResultMessage> UpsertStoreAsync(Store entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = await NewStoreCodeAsync();
                }

                entity.CreatedAt = DateTime.Now;
                entity.StoreProducts = await _context.Products.AsNoTracking()
                    .Select(a => new StoreProduct { ProductId = a.Id })
                    .ToListAsync();
                await _context.Stores.AddAsync(entity);
            }
            else
            {
                var existing = await _context.Stores.FirstOrDefaultAsync(a => a.Id == entity.Id);
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
