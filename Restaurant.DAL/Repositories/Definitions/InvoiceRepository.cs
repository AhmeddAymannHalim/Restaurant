using Restaurant.DAL.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IJournalEntryRepository _journals;

    public InvoiceRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _journals = journals;
    }

    public async Task<PaginatedResult<Invoice>> GetPagedInvoicesAsync(PageRequest request)
    {
        var query = _context.Invoices.AsNoTracking()
            .Include(a => a.Supplier).ThenInclude(a => a.Account)
            .Include(a => a.Customer).ThenInclude(a => a.Account)
            .Include(a => a.Store)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.InvoiceCode.ToLower().Contains(search)
                || (a.Supplier != null && (a.Supplier.Account.ArName.ToLower().Contains(search)
                    || a.Supplier.Account.EnName.ToLower().Contains(search)))
                || (a.Customer != null && (a.Customer.Account.ArName.ToLower().Contains(search)
                    || a.Customer.Account.EnName.ToLower().Contains(search))));
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
            .Include(a => a.Supplier).ThenInclude(a => a.Account)
            .Include(a => a.Customer).ThenInclude(a => a.Account)
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
        var account = await _context.Suppliers.AsNoTracking()
            .Where(a => a.Id == supplierId)
            .Select(a => a.Account)
            .FirstOrDefaultAsync();

        return account?.BalanceValue ?? 0;
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var account = await _context.Customers.AsNoTracking()
            .Where(a => a.Id == customerId)
            .Select(a => a.Account)
            .FirstOrDefaultAsync();

        return account?.BalanceValue ?? 0;
    }

    public async Task<ResultMessage> UpsertInvoiceAsync(Invoice entity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

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
                invoice.CustomerId = entity.CustomerId;
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

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Invoice, invoice.Id);
            var journalLines = BuildJournalLines(invoice).ToList();
            var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Invoice,
                invoice.Id,
                invoice.InvoiceDate,
                $"{invoice.InvoiceType} {invoice.InvoiceCode}",
                journalLines,
                saveChanges: false);
            if (!posting.Success)
            {
                return posting;
            }

            await _context.SaveChangesAsync();
            await ErpBillMirror.SyncAsync(_context, invoice, journalLines);
            await transaction.CommitAsync();

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
        await using var transaction = await _context.Database.BeginTransactionAsync();

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

            await _journals.RemoveSourceEntriesAsync(JournalSourceType.Invoice, invoice.Id);
            await ErpBillMirror.RemoveAsync(_context, invoice);

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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

        var partnerValid = IsSupplierInvoice(invoice.InvoiceType)
            ? invoice.SupplierId.HasValue && await _context.Suppliers.AnyAsync(a => a.Id == invoice.SupplierId.Value)
            : invoice.CustomerId.HasValue && await _context.Customers.AnyAsync(a => a.Id == invoice.CustomerId.Value);

        var valid = partnerValid
            && await _context.Stores.AnyAsync(a => a.Id == invoice.StoreId)
            && await _context.Products.CountAsync(a => productIds.Contains(a.Id)) == productIds.Count
            && await _context.UnitPrices.CountAsync(a => unitIds.Contains(a.Id)) == unitIds.Count;

        return valid ? null : new ResultMessage
        {
            Success = false,
            Message = "RecordNotFound"
        };
    }

    private static bool IsSupplierInvoice(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.Returns;
    }

    // Purchases and sales returns bring stock in, sales and purchase returns take it out.
    private static int StockSign(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.SalesReturn ? 1 : -1;
    }

    // Purchases raise what we owe the supplier, sales raise what the customer owes us.
    private static int BalanceSign(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1;
    }

    private async Task ApplyAsync(Invoice invoice)
    {
        await MoveStockAsync(invoice, StockSign(invoice.InvoiceType));

        if (invoice.InvoiceType == InvoiceType.Purchases)
        {
            foreach (var item in invoice.InvoiceItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product is not null)
                {
                    product.Cost = item.Price;
                }
            }
        }

        await AdjustPartnerBalanceAsync(invoice, BalanceSign(invoice.InvoiceType));
    }

    private async Task TakeBackAsync(Invoice invoice)
    {
        await MoveStockAsync(invoice, -StockSign(invoice.InvoiceType));
        await AdjustPartnerBalanceAsync(invoice, -BalanceSign(invoice.InvoiceType));
    }

    private async Task MoveStockAsync(Invoice invoice, int sign)
    {
        foreach (var item in invoice.InvoiceItems)
        {
            var stock = await GetStockAsync(invoice.StoreId, item.ProductId);
            var delta = sign * item.Quantity;
            stock.Quantity += delta;
            await _context.StoreProductMovements.AddAsync(new StoreProductMovement
            {
                StoreId = invoice.StoreId,
                ProductId = item.ProductId,
                QuantityDelta = delta,
                Source = StockMovementSource.Invoice,
                ReferenceCode = invoice.InvoiceCode
            });
        }
    }

    private async Task AdjustPartnerBalanceAsync(Invoice invoice, int sign)
    {
        if (IsSupplierInvoice(invoice.InvoiceType))
        {
            var supplier = await _context.Suppliers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == invoice.SupplierId);
            if (supplier is not null)
            {
                supplier.Account.Adjust(sign * invoice.Remaining);
            }

            return;
        }

        var customer = await _context.Customers.Include(a => a.Account).FirstOrDefaultAsync(a => a.Id == invoice.CustomerId);
        if (customer is not null)
        {
            customer.Account.Adjust(sign * invoice.Remaining);
        }
    }

    private static IEnumerable<JournalLineRequest> BuildJournalLines(Invoice invoice)
    {
        var total = invoice.TotalPrice;
        var paid = invoice.Paid;
        var remaining = invoice.Remaining;

        return invoice.InvoiceType switch
        {
            InvoiceType.Purchases => new[]
            {
                new JournalLineRequest(AccountCodes.Inventory, total, 0),
                new JournalLineRequest(AccountCodes.AccountsPayable, 0, remaining),
                new JournalLineRequest(AccountCodes.Cash, 0, paid)
            },
            InvoiceType.Returns => new[]
            {
                new JournalLineRequest(AccountCodes.AccountsPayable, remaining, 0),
                new JournalLineRequest(AccountCodes.Cash, paid, 0),
                new JournalLineRequest(AccountCodes.PurchaseReturns, 0, total)
            },
            InvoiceType.Sales => new[]
            {
                new JournalLineRequest(AccountCodes.AccountsReceivable, remaining, 0),
                new JournalLineRequest(AccountCodes.Cash, paid, 0),
                new JournalLineRequest(AccountCodes.SalesRevenue, 0, total)
            },
            _ => new[]
            {
                new JournalLineRequest(AccountCodes.SalesReturns, total, 0),
                new JournalLineRequest(AccountCodes.AccountsReceivable, 0, remaining),
                new JournalLineRequest(AccountCodes.Cash, 0, paid)
            }
        };
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
