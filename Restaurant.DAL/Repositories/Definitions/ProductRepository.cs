using Shared.Helpers;
using Restaurant.DAL.Helpers;
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
    private readonly IStockRepository _stock;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
        _stock = new StockRepository(context);
    }

    public async Task<PaginatedResult<Product>> GetPagedProductsAsync(PageRequest request)
    {
        var query = _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
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

        var pagedStores = await _context.Stores.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var pagedQuantities = await _stock.QuantitiesAsync();

        foreach (var product in data)
        {
            product.Stocks = pagedStores.Select(store => new ProductStock
            {
                StoreId = store.Id,
                Store = store,
                Quantity = _stock.FindQuantity(pagedQuantities, store.Id, product.Id)
            }).ToList();
        }

        return PaginatedResult<Product>.Create(data, total, request);
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (product is not null)
        {
            var stores = await _context.Stores.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
            var quantities = await _stock.QuantitiesAsync();

            product.Stocks = stores.Select(store => new ProductStock
            {
                StoreId = store.Id,
                Store = store,
                Quantity = _stock.FindQuantity(quantities, store.Id, product.Id)
            }).ToList();
        }

        return product;
    }

    public async Task<ResultMessage> UpsertProductAsync(Product entity, IEnumerable<ProductStock> stocks)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (entity.Id == 0)
            {
                await _context.Products.AddAsync(entity);
                await _context.SaveChangesAsync();
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
                await _context.SaveChangesAsync();
            }

            var current = await _stock.QuantitiesAsync();
            foreach (var stock in stocks)
            {
                if (_stock.FindQuantity(current, stock.StoreId, entity.Id) == stock.Quantity)
                {
                    continue;
                }

                var row = await _stock.RowAsync(stock.StoreId, entity.Id);
                row.Quantity = stock.Quantity;
            }

            // a store taken off the list no longer holds this product
            var keptStoreIds = stocks.Select(a => a.StoreId).ToList();
            foreach (var old in current)
            {
                var isThisProduct = old.ProductId == entity.Id;
                var storeWasRemoved = !keptStoreIds.Contains(old.StoreId);
                if (isThisProduct && storeWasRemoved && old.Quantity != 0)
                {
                    var row = await _stock.RowAsync(old.StoreId, entity.Id);
                    row.Quantity = 0;
                }
            }

            await _context.SaveChangesAsync();
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

    public async Task<ResultMessage> DeleteProductAsync(int id)
    {
        try
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            var itemCode = BillSupportRepository.ItemCodePrefix + id;
            if (await _stock.Rows().AnyAsync(a => a.Item.Code == itemCode && a.Quantity > 0))
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
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
