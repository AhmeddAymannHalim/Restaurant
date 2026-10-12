using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<Invoice>> GetPagedInvoicesAsync(PageRequest request)
    {
        var query = _context.Invoices.AsNoTracking()
            .Include(a => a.Supplier)
            .Include(a => a.Store)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.InvoiceCode.ToLower().Contains(search)
                || a.Supplier.ArName.ToLower().Contains(search)
                || a.Supplier.EnName.ToLower().Contains(search));
        }

        if (Enum.TryParse<InvoiceType>(request.Type, out var type))
        {
            query = query.Where(a => a.InvoiceType == type);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.InvoiceDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.InvoiceDate < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.InvoiceDate)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<Invoice>.Create(data, total, request);
    }

    public async Task<Invoice> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(a => a.Supplier)
            .Include(a => a.Store)
            .Include(a => a.InvoiceItems).ThenInclude(a => a.Product)
            .Include(a => a.InvoiceItems).ThenInclude(a => a.UnitPrice)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string> GetNextCodeAsync()
    {
        var last = await _context.Invoices.AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Select(a => a.InvoiceCode)
            .FirstOrDefaultAsync();

        return int.TryParse(last, out var number) ? (number + 1).ToString() : "1";
    }

    public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
    {
        return await _context.Suppliers.AsNoTracking()
            .Where(a => a.Id == supplierId)
            .Select(a => a.Balance)
            .FirstOrDefaultAsync();
    }

    public async Task<ResultMessage> UpsertInvoiceAsync(Invoice entity)
    {
        try
        {
            var items = (entity.InvoiceItems ?? new List<InvoiceItem>())
                .Where(a => a.ProductId > 0 && a.Quantity > 0)
                .ToList();
            if (items.Count == 0)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "InvoiceNeedsItems"
                };
            }

            var error = await CheckReferencesAsync(entity, items);
            if (error is not null)
            {
                return error;
            }

            var lines = items.Select(a => new InvoiceItem
            {
                ProductId = a.ProductId,
                UnitPriceId = a.UnitPriceId,
                Quantity = a.Quantity,
                Price = a.Price
            }).ToList();

            Invoice invoice;
            if (entity.Id == 0)
            {
                invoice = entity;
                invoice.InvoiceItems = lines;
                invoice.InvoiceCode = await GetNextCodeAsync();
                invoice.InvoiceDate = DateTime.Now;
                await _context.Invoices.AddAsync(invoice);
            }
            else
            {
                invoice = await _context.Invoices.Include(a => a.InvoiceItems).FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (invoice is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                await TakeBackAsync(invoice);
                _context.InvoiceItems.RemoveRange(invoice.InvoiceItems);

                invoice.InvoiceType = entity.InvoiceType;
                invoice.PayType = entity.PayType;
                invoice.SupplierId = entity.SupplierId;
                invoice.StoreId = entity.StoreId;
                invoice.Discount = entity.Discount;
                invoice.Paid = entity.Paid;
            }

            invoice.InvoiceItems = lines;
            invoice.TotalPrice = invoice.InvoiceItems.Sum(a => a.Total) - invoice.Discount;
            invoice.Remaining = invoice.TotalPrice - invoice.Paid;

            await ApplyAsync(invoice);

            var negative = _context.ChangeTracker.Entries<StoreProduct>().Any(a => a.State != EntityState.Unchanged
                && a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return new ResultMessage
                {
                    Success = false,
                    Message = "InsufficientStock"
                };
            }

            await _context.SaveChangesAsync();

            return new ResultMessage
            {
                Success = true,
                Data = invoice.InvoiceCode
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "ConcurrentUpdate"
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

    public async Task<ResultMessage> DeleteInvoiceAsync(int id)
    {
        try
        {
            var invoice = await _context.Invoices.Include(a => a.InvoiceItems).FirstOrDefaultAsync(a => a.Id == id);
            if (invoice is null)
            {
                return new ResultMessage { Success = true };
            }

            await TakeBackAsync(invoice);

            var negative = _context.ChangeTracker.Entries<StoreProduct>().Any(a => a.State != EntityState.Unchanged
                && a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return new ResultMessage
                {
                    Success = false,
                    Message = "InsufficientStock"
                };
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage
            {
                Success = false,
                Message = "ConcurrentUpdate"
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

    private async Task<ResultMessage> CheckReferencesAsync(Invoice invoice, List<InvoiceItem> items)
    {
        var productIds = items.Select(a => a.ProductId).Distinct().ToList();
        var unitIds = items.Select(a => a.UnitPriceId).Distinct().ToList();

        var valid = await _context.Suppliers.AnyAsync(a => a.Id == invoice.SupplierId)
            && await _context.Stores.AnyAsync(a => a.Id == invoice.StoreId)
            && await _context.Products.CountAsync(a => productIds.Contains(a.Id)) == productIds.Count
            && await _context.UnitPrices.CountAsync(a => unitIds.Contains(a.Id)) == unitIds.Count;

        return valid ? null : new ResultMessage
        {
            Success = false,
            Message = "RecordNotFound"
        };
    }

    private async Task ApplyAsync(Invoice invoice)
    {
        var isPurchase = invoice.InvoiceType == InvoiceType.Purchases;

        foreach (var item in invoice.InvoiceItems)
        {
            var stock = await GetStockAsync(invoice.StoreId, item.ProductId);
            stock.Quantity += isPurchase ? item.Quantity : -item.Quantity;

            if (isPurchase)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                product.Cost = item.Price;
            }
        }

        var supplier = await _context.Suppliers.FindAsync(invoice.SupplierId);
        supplier.Balance += isPurchase ? invoice.Remaining : -invoice.Remaining;
    }

    private async Task TakeBackAsync(Invoice invoice)
    {
        var isPurchase = invoice.InvoiceType == InvoiceType.Purchases;

        foreach (var item in invoice.InvoiceItems)
        {
            var stock = await GetStockAsync(invoice.StoreId, item.ProductId);
            stock.Quantity -= isPurchase ? item.Quantity : -item.Quantity;
        }

        var supplier = await _context.Suppliers.FindAsync(invoice.SupplierId);
        supplier.Balance -= isPurchase ? invoice.Remaining : -invoice.Remaining;
    }

    private async Task<StoreProduct> GetStockAsync(int storeId, int productId)
    {
        var stock = await _context.StoreProducts.FirstOrDefaultAsync(a => a.StoreId == storeId && a.ProductId == productId)
            ?? _context.StoreProducts.Local.FirstOrDefault(a => a.StoreId == storeId && a.ProductId == productId);
        if (stock is null)
        {
            stock = new StoreProduct
            {
                StoreId = storeId,
                ProductId = productId
            };
            await _context.StoreProducts.AddAsync(stock);
        }

        return stock;
    }
}
