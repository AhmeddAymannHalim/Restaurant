using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Product>> GetPagedProductsAsync(PageRequest request)
    {
        var query = _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .Include(a => a.StoreProducts)
            .AsQueryable();

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

        return PaginatedResult<Product>.Create(data, total, request);
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .Include(a => a.StoreProducts).ThenInclude(a => a.Store)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> UpsertProductAsync(Product entity, IEnumerable<StoreProduct> stocks)
    {
        try
        {
            if (entity.Id == 0)
            {
                var storeIds = await _context.Stores.AsNoTracking().Select(a => a.Id).ToListAsync();
                entity.StoreProducts = storeIds.Select(storeId => new StoreProduct
                {
                    StoreId = storeId,
                    Quantity = stocks.Where(a => a.StoreId == storeId).Select(a => a.Quantity).FirstOrDefault()
                }).ToList();
                await _context.Products.AddAsync(entity);
            }
            else
            {
                if (!await _context.Products.AnyAsync(a => a.Id == entity.Id))
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                _context.Products.Update(entity);

                var current = await _context.StoreProducts.Where(a => a.ProductId == entity.Id).ToListAsync();
                foreach (var stock in stocks)
                {
                    var row = current.FirstOrDefault(a => a.StoreId == stock.StoreId);
                    if (row is null)
                    {
                        await _context.StoreProducts.AddAsync(new StoreProduct
                        {
                            ProductId = entity.Id,
                            StoreId = stock.StoreId,
                            Quantity = stock.Quantity
                        });
                    }
                    else
                    {
                        row.Quantity = stock.Quantity;
                    }
                }
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

    public async Task<ResultMessage> DeleteProductAsync(int id)
    {
        try
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            if (await _context.StoreProducts.AnyAsync(a => a.ProductId == id && a.Quantity > 0))
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "CannotDeleteHasStock"
                };
            }

            _context.Products.Remove(entity);
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
