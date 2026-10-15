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

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
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

        await FillStocksAsync(data);

        return PaginatedResult<Product>.Create(data, total, request);
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(a => a.ProductCategory)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (product is not null)
        {
            await FillStocksAsync(new List<Product> { product });
        }

        return product;
    }

    private async Task FillStocksAsync(List<Product> products)
    {
        var stores = await _context.Stores.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        var quantities = await StoreStock.QuantitiesAsync(_context);

        foreach (var product in products)
        {
            product.Stocks = stores.Select(store => new ProductStock
            {
                StoreId = store.Id,
                Store = store,
                Quantity = quantities.GetValueOrDefault((store.Id, product.Id))
            }).ToList();
        }
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

            var current = await StoreStock.QuantitiesAsync(_context);
            foreach (var stock in stocks)
            {
                if (current.GetValueOrDefault((stock.StoreId, entity.Id)) == stock.Quantity)
                {
                    continue;
                }

                var row = await StoreStock.RowAsync(_context, stock.StoreId, entity.Id);
                row.Quantity = stock.Quantity;
            }

            // a store taken off the list no longer holds this product
            var kept = stocks.Select(a => a.StoreId).ToHashSet();
            foreach (var emptied in current.Where(a => a.Key.ProductId == entity.Id && !kept.Contains(a.Key.StoreId) && a.Value != 0))
            {
                var row = await StoreStock.RowAsync(_context, emptied.Key.StoreId, entity.Id);
                row.Quantity = 0;
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

            var itemCode = BillSupport.ItemCodePrefix + id;
            if (await StoreStock.Rows(_context).AnyAsync(a => a.Item.Code == itemCode && a.Quantity > 0))
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
