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
        var query = _context.Products.AsNoTracking().AsQueryable();

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
        return await _context.Products.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertProductAsync(Product entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.Products.AddAsync(entity);
                await _context.SaveChangesAsync();

                // Every store gets a zero-quantity row for the new product
                var stores = await _context.Stores.AsNoTracking().ToListAsync();
                foreach (var store in stores)
                {
                    await _context.StoreProducts.AddAsync(new StoreProduct
                    {
                        ProductId = entity.Id,
                        ProductName = entity.ArName,
                        StoreId = store.Id,
                        StoreName = store.ArName,
                        Quantity = 0,
                        PriceForUnit = 0
                    });
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                _context.Products.Update(entity);
                await _context.SaveChangesAsync();

                // Keep the store rows in sync with the product name
                var stores = await _context.Stores.AsNoTracking().ToListAsync();
                foreach (var store in stores)
                {
                    var storeProduct = await _context.StoreProducts
                        .FirstOrDefaultAsync(a => a.ProductId == entity.Id && a.StoreId == store.Id);
                    if (storeProduct != null)
                    {
                        storeProduct.ProductName = entity.ArName;
                    }
                    else
                    {
                        await _context.StoreProducts.AddAsync(new StoreProduct
                        {
                            ProductId = entity.Id,
                            ProductName = entity.ArName,
                            StoreId = store.Id,
                            StoreName = store.ArName,
                            Quantity = 0,
                            PriceForUnit = 0
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
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

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }
}
