using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

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

    public async Task<InvoiceViewModel> GetInvoiceByIdAsync(int id)
    {
        var bill = await _context.BillMasters.AsNoTracking()
            .Include(a => a.BillSetting)
            .Include(a => a.Store)
            .Include(a => a.BillDetails).ThenInclude(a => a.Item)
            .Include(a => a.BillDetails).ThenInclude(a => a.Unit)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (bill is null || !await BillSupport.IsInvoiceAsync(_context, bill))
        {
            return null;
        }

        var type = BillSupport.TypeOf(bill.BillSetting.BillType);
        var isArabic = LanguageHelper.IsArabic();
        var model = new InvoiceViewModel
        {
            Id = bill.Id,
            InvoiceCode = bill.Code,
            InvoiceDate = bill.Date,
            InvoiceType = type,
            PayType = bill.PayType == Shared.Enums.PayType.Later ? Shared.Enums.PayType.Later : Shared.Enums.PayType.Cash,
            StoreId = bill.StoreId ?? 0,
            StoreArName = bill.Store?.ArName,
            StoreEnName = bill.Store?.EnName,
            Discount = bill.BillDiscountValue,
            Paid = bill.AmountPaided,
            AutoPost = bill.IsEntryPosted,
            AutoStock = bill.IsStorageEffected,
            Items = bill.BillDetails
                .OrderBy(a => a.RowNumber)
                .ThenBy(a => a.Id)
                .Select(a => new InvoiceItemViewModel
                {
                    Id = a.Id,
                    ProductId = BillSupport.ProductIdOf(a.Item?.Code),
                    ProductName = isArabic ? a.Item?.ArName : a.Item?.EnName,
                    UnitPriceId = BillSupport.UnitPriceIdOf(a.Unit?.Code),
                    UnitName = isArabic ? a.Unit?.ArName : a.Unit?.EnName,
                    Quantity = a.Quantity ?? 0,
                    Price = a.UnitPrice ?? 0
                })
                .ToList()
        };

        var accountId = bill.CustomerOrSupplierAccountId;
        if (accountId.HasValue)
        {
            if (model.IsSupplierInvoice)
            {
                var supplier = await _context.Suppliers.AsNoTracking().Include(a => a.Account).FirstOrDefaultAsync(a => a.AccountId == accountId);
                model.SupplierId = supplier?.Id;
                model.SupplierArName = supplier?.Account?.ArName;
                model.SupplierEnName = supplier?.Account?.EnName;
            }
            else
            {
                var customer = await _context.Customers.AsNoTracking().Include(a => a.Account).FirstOrDefaultAsync(a => a.AccountId == accountId);
                model.CustomerId = customer?.Id;
                model.CustomerArName = customer?.Account?.ArName;
                model.CustomerEnName = customer?.Account?.EnName;
            }
        }

        return model;
    }

    public async Task<string> GetNextCodeAsync(InvoiceType type)
    {
        var (_, billType, arName, enName, _) = BillSupport.Types[type];
        var setting = await BillSupport.GetBillSettingAsync(_context, billType, arName, enName);

        return await BillSupport.NextBillCodeAsync(_context, setting.Id);
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

    public async Task<ResultMessage> UpsertInvoiceAsync(InvoiceViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var items = (model.Items ?? new List<InvoiceItemViewModel>())
                .Where(a => a.ProductId > 0 && a.Quantity > 0)
                .ToList();
            if (items.Count == 0)
            {
                return Fail("InvoiceNeedsItems");
            }

            var partner = await FindPartnerAsync(model);
            var error = await CheckReferencesAsync(model, items, partner);
            if (error is not null)
            {
                return error;
            }

            var (_, billType, arName, enName, entryType) = BillSupport.Types[model.InvoiceType];

            var setting = await BillSupport.GetBillSettingAsync(_context, billType, arName, enName);
            var entrySetting = await BillSupport.GetEntrySettingAsync(_context, entryType, arName, enName);
            var itemIds = new Dictionary<int, int>();
            var unitIds = new Dictionary<int, int>();
            foreach (var item in items)
            {
                if (!itemIds.ContainsKey(item.ProductId))
                {
                    itemIds[item.ProductId] = await BillSupport.GetItemIdAsync(_context, item.ProductId);
                }

                if (!unitIds.ContainsKey(item.UnitPriceId))
                {
                    unitIds[item.UnitPriceId] = await BillSupport.GetUnitIdAsync(_context, item.UnitPriceId);
                }
            }

            var costs = await _context.Products.AsNoTracking()
                .Where(a => itemIds.Keys.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.Cost);

            BillMaster bill;
            if (model.Id == 0)
            {
                bill = new BillMaster
                {
                    Code = await GetNextCodeAsync(model.InvoiceType),
                    Date = DateTime.Now,
                    CurrencyRate = 1,
                    BillSettingId = setting.Id,
                    CreatedAt = DateTime.Now
                };
                bill.CodeWithoutSuffix = bill.Code;
                bill.BillDailyNo = await _context.BillMasters.CountAsync(a => a.BillSettingId == setting.Id && a.Date.Date == bill.Date.Date) + 1;
                await _context.BillMasters.AddAsync(bill);
            }
            else
            {
                bill = await _context.BillMasters
                    .Include(a => a.BillSetting)
                    .Include(a => a.BillDetails).ThenInclude(a => a.Item)
                    .Include(a => a.BillDetails).ThenInclude(a => a.Unit)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);
                if (bill is null)
                {
                    return Fail("RecordNotFound");
                }

                if (!await BillSupport.IsInvoiceAsync(_context, bill))
                {
                    return Fail("BillNotEditable");
                }

                await TakeBackAsync(bill);
                await RemoveEntriesAsync(bill.Id);
                _context.BillDetails.RemoveRange(bill.BillDetails);

                bill.BillSettingId = setting.Id;
                bill.UpdatedAt = DateTime.Now;
            }

            var details = items.Select((line, index) => new BillDetail
            {
                RowNumber = (byte)Math.Min(index + 1, byte.MaxValue),
                ItemId = itemIds[line.ProductId],
                UnitId = unitIds[line.UnitPriceId],
                StoreId = model.StoreId,
                Quantity = line.Quantity,
                UnitPrice = line.Price,
                TransRate = 1,
                TotalAmount = Math.Round(line.Quantity * line.Price, 2),
                UnitCostAtPosting = costs.GetValueOrDefault(line.ProductId),
                CreatedAt = DateTime.Now
            }).ToList();

            var itemsTotal = details.Sum(a => a.TotalAmount ?? 0);
            var discount = model.Discount;
            var total = itemsTotal - discount;
            var paid = model.Paid;
            var remaining = total - paid;
            var isSale = model.InvoiceType is InvoiceType.Sales or InvoiceType.SalesReturn;
            var cost = isSale ? details.Sum(a => (a.UnitCostAtPosting ?? 0) * (a.Quantity ?? 0)) : (decimal?)null;

            bill.StoreId = model.StoreId;
            bill.CustomerOrSupplierAccountId = partner.AccountId;
            bill.TotalAmountBeforeDiscountOrExtra = itemsTotal;
            bill.TotalItemsAmountAfterDiscountAndExtra = itemsTotal;
            bill.BillDiscountValue = discount;
            bill.TotalDiscountValueOnBillAndItems = discount;
            bill.TotalBillAmountAfterDiscountAndExtra = total;
            bill.PayType = model.PayType == Shared.Enums.PayType.Later ? Shared.Enums.PayType.Later : Shared.Enums.PayType.Cash;
            bill.AmountPaided = paid;
            bill.CasherRemainingAmount = remaining;
            bill.IsStorageEffected = model.AutoStock;
            bill.IsEntryPosted = model.AutoPost;
            bill.BillCost = cost;
            bill.BillProfit = cost.HasValue ? total - cost : null;
            bill.BillDetails = details;

            await ApplyAsync(bill, model.InvoiceType, partner, items, itemIds, unitIds, remaining);

            var negative = _context.ChangeTracker.Entries<StoreItemUnit>().Any(a => a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return Fail("InsufficientStock");
            }

            if (model.AutoPost)
            {
                var journalLines = BuildJournalLines(model.InvoiceType, total, paid, remaining, partner.AccountId).ToList();
                var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Invoice, 0, bill.Date, bill.Code, journalLines, saveChanges: false);
                if (!posting.Success)
                {
                    _context.ChangeTracker.Clear();
                    return posting;
                }

                await AddEntryAsync(bill, model.InvoiceType, entrySetting, journalLines, paid);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage
            {
                Success = true,
                Data = bill.Code
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail("ConcurrentUpdate");
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    public async Task<ResultMessage> DeleteInvoiceAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var bill = await _context.BillMasters
                .Include(a => a.BillSetting)
                .Include(a => a.BillDetails).ThenInclude(a => a.Item)
                .Include(a => a.BillDetails).ThenInclude(a => a.Unit)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (bill is null)
            {
                return new ResultMessage { Success = true };
            }

            if (!await BillSupport.IsInvoiceAsync(_context, bill))
            {
                return Fail("BillNotEditable");
            }

            await TakeBackAsync(bill);

            var negative = _context.ChangeTracker.Entries<StoreItemUnit>().Any(a => a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return Fail("InsufficientStock");
            }

            await RemoveEntriesAsync(bill.Id);
            _context.BillDetails.RemoveRange(bill.BillDetails);
            _context.BillMasters.Remove(bill);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail("ConcurrentUpdate");
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    private sealed record Partner(int? SupplierId, int? CustomerId, int? AccountId);

    private async Task<Partner> FindPartnerAsync(InvoiceViewModel model)
    {
        if (model.IsSupplierInvoice)
        {
            var supplier = model.SupplierId.HasValue
                ? await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.SupplierId.Value)
                : null;

            return new Partner(supplier?.Id, null, supplier?.AccountId);
        }

        var customer = model.CustomerId.HasValue
            ? await _context.Customers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.CustomerId.Value)
            : null;

        return new Partner(null, customer?.Id, customer?.AccountId);
    }

    private async Task<ResultMessage> CheckReferencesAsync(InvoiceViewModel model, List<InvoiceItemViewModel> items, Partner partner)
    {
        var productIds = items.Select(a => a.ProductId).Distinct().ToList();
        var unitIds = items.Select(a => a.UnitPriceId).Distinct().ToList();

        var valid = partner.AccountId.HasValue
            && await _context.Stores.AnyAsync(a => a.Id == model.StoreId)
            && await _context.Products.CountAsync(a => productIds.Contains(a.Id)) == productIds.Count
            && await _context.UnitPrices.CountAsync(a => unitIds.Contains(a.Id)) == unitIds.Count;

        return valid ? null : Fail("RecordNotFound");
    }

    private static int StockSign(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.SalesReturn ? 1 : -1;
    }

    private static int BalanceSign(InvoiceType type)
    {
        return type is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1;
    }

    private async Task ApplyAsync(BillMaster bill, InvoiceType type, Partner partner, List<InvoiceItemViewModel> items,
        Dictionary<int, int> itemIds, Dictionary<int, int> unitIds, decimal remaining)
    {
        var sign = StockSign(type);
        var storeId = bill.StoreId.Value;

        foreach (var item in items)
        {
            await BillSupport.EnsureItemUnitAsync(_context, itemIds[item.ProductId], unitIds[item.UnitPriceId]);
            if (bill.IsStorageEffected)
            {
                await MoveStockAsync(storeId, item.ProductId, sign * item.Quantity, bill.Code);
            }
        }

        if (type == InvoiceType.Purchases && bill.IsStorageEffected)
        {
            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product is not null)
                {
                    product.Cost = item.Price;
                }
            }
        }

        if (bill.IsEntryPosted)
        {
            await AdjustPartnerBalanceAsync(partner.AccountId, BalanceSign(type) * remaining);
        }
    }

    private async Task TakeBackAsync(BillMaster bill)
    {
        var type = BillSupport.TypeOf(bill.BillSetting.BillType);
        var sign = -StockSign(type);
        var storeId = bill.StoreId ?? 0;

        foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
        {
            var productId = BillSupport.ProductIdOf(detail.Item?.Code);
            if (productId == 0 || detail.ItemId is null || detail.UnitId is null)
            {
                continue;
            }

            await MoveStockAsync(storeId, productId, sign * (detail.Quantity ?? 0), bill.Code);
        }

        if (bill.IsEntryPosted)
        {
            await AdjustPartnerBalanceAsync(bill.CustomerOrSupplierAccountId, -BalanceSign(type) * bill.CasherRemainingAmount);
        }
    }

    public async Task<bool> IsInvoiceAsync(int id)
    {
        var billTypes = BillSupport.InvoiceBillTypes;

        return await _context.BillMasters.AnyAsync(a => a.Id == id && billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id));
    }

    // like the ERP: the bill setting of each type says whether the entry and the stock effect happen on save
    public async Task<List<PostingDefaultViewModel>> GetPostingDefaultsAsync()
    {
        var settings = await _context.BillSettings.AsNoTracking()
            .Where(a => BillSupport.InvoiceBillTypes.Contains(a.BillType))
            .OrderBy(a => a.Id)
            .Select(a => new { a.BillType, a.EntryGenerate, a.StorageEffect })
            .ToListAsync();

        return BillSupport.Types.Select(type =>
        {
            var setting = settings.FirstOrDefault(a => a.BillType == type.Value.BillType);

            return new PostingDefaultViewModel
            {
                Type = type.Key,
                AutoPost = setting is null || setting.EntryGenerate == EntryGenerate.AutomaticEntryCreated,
                AutoStock = setting is null || setting.StorageEffect == StorageEffect.AutoEffected
            };
        }).ToList();
    }

    public async Task<List<int>> GetUnpostedInvoiceIdsAsync()
    {
        var billTypes = BillSupport.InvoiceBillTypes;

        return await _context.BillMasters.AsNoTracking()
            .Where(a => (!a.IsEntryPosted || !a.IsStorageEffected) && billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id))
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync();
    }

    // like the ERP "post": writes the entry and moves the stock, whichever of the two has not happened yet
    public async Task<ResultMessage> PostInvoiceAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var bill = await _context.BillMasters
                .Include(a => a.BillSetting)
                .Include(a => a.BillDetails).ThenInclude(a => a.Item)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (bill is null || !await BillSupport.IsInvoiceAsync(_context, bill))
            {
                return Fail("RecordNotFound");
            }

            if (bill.IsEntryPosted && bill.IsStorageEffected)
            {
                return Fail("BillAlreadyPosted");
            }

            var type = BillSupport.TypeOf(bill.BillSetting.BillType);
            var remaining = bill.CasherRemainingAmount;

            if (!bill.IsEntryPosted)
            {
                var (_, _, arName, enName, entryType) = BillSupport.Types[type];
                var entrySetting = await BillSupport.GetEntrySettingAsync(_context, entryType, arName, enName);
                var journalLines = BuildJournalLines(type, bill.TotalBillAmountAfterDiscountAndExtra, bill.AmountPaided, remaining, bill.CustomerOrSupplierAccountId).ToList();
                var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Invoice, 0, bill.Date, bill.Code, journalLines, saveChanges: false);
                if (!posting.Success)
                {
                    return posting;
                }

                await AddEntryAsync(bill, type, entrySetting, journalLines, bill.AmountPaided);
                await AdjustPartnerBalanceAsync(bill.CustomerOrSupplierAccountId, BalanceSign(type) * remaining);
                bill.IsEntryPosted = true;
            }

            if (!bill.IsStorageEffected)
            {
                var sign = StockSign(type);
                foreach (var detail in bill.BillDetails)
                {
                    var productId = BillSupport.ProductIdOf(detail.Item?.Code);
                    if (productId == 0)
                    {
                        continue;
                    }

                    await MoveStockAsync(bill.StoreId ?? 0, productId, sign * (detail.Quantity ?? 0), bill.Code);
                    if (type == InvoiceType.Purchases && detail.UnitPrice.HasValue)
                    {
                        var product = await _context.Products.FindAsync(productId);
                        if (product is not null)
                        {
                            product.Cost = detail.UnitPrice.Value;
                        }
                    }
                }

                if (_context.ChangeTracker.Entries<StoreItemUnit>().Any(a => a.Entity.Quantity < 0))
                {
                    _context.ChangeTracker.Clear();
                    return Fail("InsufficientStock");
                }

                bill.IsStorageEffected = true;
            }

            bill.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    private async Task MoveStockAsync(int storeId, int productId, decimal delta, string code)
    {
        await StoreStock.AdjustAsync(_context, storeId, productId, delta, allowNegative: true);
        await _context.StoreProductMovements.AddAsync(new StoreProductMovement
        {
            StoreId = storeId,
            ProductId = productId,
            QuantityDelta = delta,
            Source = StockMovementSource.Invoice,
            ReferenceCode = code
        });
    }

    private async Task AdjustPartnerBalanceAsync(int? accountId, decimal delta)
    {
        if (!accountId.HasValue)
        {
            return;
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId.Value);
        account?.Adjust(delta);
    }

    private static IEnumerable<JournalLineRequest> BuildJournalLines(InvoiceType type, decimal total, decimal paid, decimal remaining, int? partnerAccountId)
    {
        return type switch
        {
            InvoiceType.Purchases => new[]
            {
                new JournalLineRequest(AccountCodes.Inventory, total, 0),
                new JournalLineRequest(AccountCodes.AccountsPayable, 0, remaining, null, partnerAccountId),
                new JournalLineRequest(AccountCodes.Cash, 0, paid)
            },
            InvoiceType.Returns => new[]
            {
                new JournalLineRequest(AccountCodes.AccountsPayable, remaining, 0, null, partnerAccountId),
                new JournalLineRequest(AccountCodes.Cash, paid, 0),
                new JournalLineRequest(AccountCodes.PurchaseReturns, 0, total)
            },
            InvoiceType.Sales => new[]
            {
                new JournalLineRequest(AccountCodes.AccountsReceivable, remaining, 0, null, partnerAccountId),
                new JournalLineRequest(AccountCodes.Cash, paid, 0),
                new JournalLineRequest(AccountCodes.SalesRevenue, 0, total)
            },
            _ => new[]
            {
                new JournalLineRequest(AccountCodes.SalesReturns, total, 0),
                new JournalLineRequest(AccountCodes.AccountsReceivable, 0, remaining, null, partnerAccountId),
                new JournalLineRequest(AccountCodes.Cash, 0, paid)
            }
        };
    }

    private async Task AddEntryAsync(BillMaster bill, InvoiceType type, EntrySetting entrySetting, IEnumerable<JournalLineRequest> journalLines, decimal paid)
    {
        var requested = journalLines
            .Where(a => a.Debit > 0 || a.Credit > 0)
            .ToList();
        if (requested.Count == 0)
        {
            return;
        }

        var codes = requested.Where(a => a.AccountId is null).Select(a => a.AccountCode).Distinct().ToList();
        var accounts = await _context.Accounts.AsNoTracking()
            .Where(a => codes.Contains(a.Code))
            .ToDictionaryAsync(a => a.Code, a => a.Id);
        if (accounts.Count != codes.Count)
        {
            await _journals.EnsureDefaultAccountsAsync();
            accounts = await _context.Accounts.AsNoTracking()
                .Where(a => codes.Contains(a.Code))
                .ToDictionaryAsync(a => a.Code, a => a.Id);
        }

        await _context.EntryMasters.AddAsync(new EntryMaster
        {
            Code = bill.Code,
            EntryDate = bill.Date,
            TotalBalance = requested.Sum(a => Math.Round(a.Debit, 2)),
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = $"{type} {bill.Code}",
            AccountBoxOrBankId = paid > 0 && accounts.TryGetValue(AccountCodes.Cash, out var cashId) ? cashId : null,
            Bill = bill,
            EntrySetting = entrySetting,
            CreatedAt = DateTime.Now,
            EntryDetails = requested.Select((line, index) => new EntryDetail
            {
                EntryRowCode = (short)(index + 1),
                AccountId = line.AccountId ?? accounts[line.AccountCode],
                Debit = Math.Round(line.Debit, 2),
                Credit = Math.Round(line.Credit, 2),
                EntryDetailsRemarks = line.Description,
                CreatedAt = DateTime.Now
            }).ToList()
        });
    }

    private async Task RemoveEntriesAsync(int billId)
    {
        var entries = await _context.EntryMasters
            .Include(a => a.EntryDetails)
            .Where(a => a.BillId == billId)
            .ToListAsync();

        _context.EntryDetails.RemoveRange(entries.SelectMany(a => a.EntryDetails));
        _context.EntryMasters.RemoveRange(entries);
    }

    private static ResultMessage Fail(string message)
    {
        return new ResultMessage
        {
            Success = false,
            Message = message
        };
    }
}
