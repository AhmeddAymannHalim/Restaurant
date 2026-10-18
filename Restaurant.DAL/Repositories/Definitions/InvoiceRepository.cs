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
            .Include(a => a.CostCenter)
            .Include(a => a.Currency)
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
            CostCenterId = bill.CostCenterId,
            CostCenterArName = bill.CostCenter?.ArName,
            CostCenterEnName = bill.CostCenter?.EnName,
            CurrencyId = bill.CurrencyId,
            CurrencyArName = bill.Currency?.ArName,
            CurrencyEnName = bill.Currency?.EnName,
            CurrencyRate = bill.CurrencyRate,
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
        var typeInfo = BillSupport.Types[type];
        var billType = typeInfo.BillType;
        var arName = typeInfo.ArName;
        var enName = typeInfo.EnName;
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
                return new ResultMessage { Success = false, Message = "InvoiceNeedsItems" };
            }

            Partner partner;
            if (!model.HasPartner)
            {
                partner = new Partner(null, null, null);
            }
            else if (model.IsSupplierInvoice)
            {
                var supplier = model.SupplierId.HasValue
                    ? await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.SupplierId.Value)
                    : null;
                partner = new Partner(supplier?.Id, null, supplier?.AccountId);
            }
            else
            {
                var customer = model.CustomerId.HasValue
                    ? await _context.Customers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.CustomerId.Value)
                    : null;
                partner = new Partner(null, customer?.Id, customer?.AccountId);
            }

            var productIds = items.Select(a => a.ProductId).Distinct().ToList();
            var unitPriceIds = items.Select(a => a.UnitPriceId).Distinct().ToList();

            var referencesValid = (!model.HasPartner || partner.AccountId.HasValue)
                && await _context.Stores.AnyAsync(a => a.Id == model.StoreId)
                && await _context.Products.CountAsync(a => productIds.Contains(a.Id)) == productIds.Count
                && await _context.UnitPrices.CountAsync(a => unitPriceIds.Contains(a.Id)) == unitPriceIds.Count
                && (!model.CostCenterId.HasValue || await _context.CostCenters.AnyAsync(a => a.Id == model.CostCenterId.Value))
                && (!model.CurrencyId.HasValue || await _context.Currencies.AnyAsync(a => a.Id == model.CurrencyId.Value));

            if (!referencesValid)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            var typeInfo = BillSupport.Types[model.InvoiceType];
            var billType = typeInfo.BillType;
            var arName = typeInfo.ArName;
            var enName = typeInfo.EnName;
            var entryType = typeInfo.Entry;

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
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                if (!await BillSupport.IsInvoiceAsync(_context, bill))
                {
                    return new ResultMessage { Success = false, Message = "BillNotEditable" };
                }

                var oldType = BillSupport.TypeOf(bill.BillSetting.BillType);
                var takeBackSign = -(oldType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
                var takeBackStoreId = bill.StoreId ?? 0;

                foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
                {
                    var takeBackProductId = BillSupport.ProductIdOf(detail.Item?.Code);
                    if (takeBackProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                    {
                        continue;
                    }

                    await StoreStock.AdjustAsync(_context, takeBackStoreId, takeBackProductId, takeBackSign * (detail.Quantity ?? 0), allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = takeBackStoreId,
                        ProductId = takeBackProductId,
                        QuantityDelta = takeBackSign * (detail.Quantity ?? 0),
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
                }

                if (bill.IsEntryPosted)
                {
                    var takeBackBalanceDelta = -(oldType is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1) * bill.CasherRemainingAmount;
                    if (bill.CustomerOrSupplierAccountId.HasValue)
                    {
                        var takeBackAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == bill.CustomerOrSupplierAccountId.Value);
                        takeBackAccount?.Adjust(takeBackBalanceDelta);
                    }
                }

                var existingEntries = await _context.EntryMasters
                    .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
                    .Where(a => a.BillId == bill.Id)
                    .ToListAsync();

                foreach (var existingDetail in existingEntries.SelectMany(a => a.EntryDetails))
                {
                    if (existingDetail.Account is not null && existingDetail.Account.AccountType is not (AccountType.Customer or AccountType.Supplier))
                    {
                        existingDetail.Account.Adjust(-(existingDetail.Debit - existingDetail.Credit));
                    }
                }

                _context.EntryDetails.RemoveRange(existingEntries.SelectMany(a => a.EntryDetails));
                _context.EntryMasters.RemoveRange(existingEntries);

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

            decimal? cost = null;
            if (isSale)
            {
                decimal saleCost = 0;
                foreach (var detail in details)
                {
                    var detailCost = detail.UnitCostAtPosting ?? 0;
                    var detailQuantity = detail.Quantity ?? 0;
                    saleCost += detailCost * detailQuantity;
                }

                cost = saleCost;
            }

            bill.StoreId = model.StoreId;
            bill.CostCenterId = model.CostCenterId;
            bill.CurrencyId = model.CurrencyId;
            bill.CurrencyRate = model.CurrencyRate ?? 1;
            bill.CustomerOrSupplierAccountId = partner.AccountId;
            bill.TotalAmountBeforeDiscountOrExtra = itemsTotal;
            bill.TotalItemsAmountAfterDiscountAndExtra = itemsTotal;
            bill.BillDiscountValue = discount;
            bill.TotalDiscountValueOnBillAndItems = discount;
            bill.TotalBillAmountAfterDiscountAndExtra = total;
            bill.PayType = model.PayType == Shared.Enums.PayType.Later ? Shared.Enums.PayType.Later : Shared.Enums.PayType.Cash;
            bill.AmountPaided = paid;
            bill.CasherRemainingAmount = remaining;
            bill.InitialPaidAmount = paid;
            bill.IsStorageEffected = model.AutoStock;
            bill.IsEntryPosted = model.AutoPost;
            bill.BillCost = cost;
            bill.BillProfit = cost.HasValue ? total - cost : null;
            bill.BillDetails = details;

            var applySign = (model.InvoiceType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
            var applyStoreId = bill.StoreId.Value;

            foreach (var item in items)
            {
                await BillSupport.EnsureItemUnitAsync(_context, itemIds[item.ProductId], unitIds[item.UnitPriceId]);
                if (bill.IsStorageEffected)
                {
                    await StoreStock.AdjustAsync(_context, applyStoreId, item.ProductId, applySign * item.Quantity, allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = applyStoreId,
                        ProductId = item.ProductId,
                        QuantityDelta = applySign * item.Quantity,
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
                }
            }

            if (model.InvoiceType == InvoiceType.Purchases && bill.IsStorageEffected)
            {
                foreach (var item in items)
                {
                    var productToUpdateCost = await _context.Products.FindAsync(item.ProductId);
                    if (productToUpdateCost is not null)
                    {
                        productToUpdateCost.Cost = item.Price;
                    }
                }
            }

            if (bill.IsEntryPosted && partner.AccountId.HasValue)
            {
                var applyBalanceDelta = (model.InvoiceType is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1) * remaining;
                var applyAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == partner.AccountId.Value);
                applyAccount?.Adjust(applyBalanceDelta);
            }

            var negative = _context.ChangeTracker.Entries<StoreItemUnit>().Any(a => a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return new ResultMessage { Success = false, Message = "InsufficientStock" };
            }

            if (model.AutoPost)
            {
                var journalLinesBase = model.InvoiceType switch
                {
                    InvoiceType.Purchases => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, total, 0),
                        new JournalLineRequest(AccountCodes.AccountsPayable, 0, remaining, null, partner.AccountId),
                        new JournalLineRequest(AccountCodes.Cash, 0, paid)
                    },
                    InvoiceType.Returns => new[]
                    {
                        new JournalLineRequest(AccountCodes.AccountsPayable, remaining, 0, null, partner.AccountId),
                        new JournalLineRequest(AccountCodes.Cash, paid, 0),
                        new JournalLineRequest(AccountCodes.PurchaseReturns, 0, total)
                    },
                    InvoiceType.Sales => new[]
                    {
                        new JournalLineRequest(AccountCodes.AccountsReceivable, remaining, 0, null, partner.AccountId),
                        new JournalLineRequest(AccountCodes.Cash, paid, 0),
                        new JournalLineRequest(AccountCodes.SalesRevenue, 0, total)
                    },
                    InvoiceType.SalesReturn => new[]
                    {
                        new JournalLineRequest(AccountCodes.SalesReturns, total, 0),
                        new JournalLineRequest(AccountCodes.AccountsReceivable, 0, remaining, null, partner.AccountId),
                        new JournalLineRequest(AccountCodes.Cash, 0, paid)
                    },
                    InvoiceType.Shortage => new[]
                    {
                        new JournalLineRequest(AccountCodes.InventoryAdjustment, total, 0),
                        new JournalLineRequest(AccountCodes.Inventory, 0, total)
                    },
                    InvoiceType.Surplus => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, total, 0),
                        new JournalLineRequest(AccountCodes.InventoryAdjustment, 0, total)
                    },
                    InvoiceType.OpeningInventory => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, total, 0),
                        new JournalLineRequest(AccountCodes.OpeningBalanceEquity, 0, total)
                    },
                    _ => Array.Empty<JournalLineRequest>()
                };

                if (model.InvoiceType == InvoiceType.Sales && cost is > 0)
                {
                    journalLinesBase = journalLinesBase.Concat(new[]
                    {
                        new JournalLineRequest(AccountCodes.Purchases, cost.Value, 0),
                        new JournalLineRequest(AccountCodes.Inventory, 0, cost.Value)
                    }).ToArray();
                }

                var journalLines = journalLinesBase.ToList();
                var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Invoice, 0, bill.Date, bill.Code, journalLines, saveChanges: false);
                if (!posting.Success)
                {
                    _context.ChangeTracker.Clear();
                    return posting;
                }

                var requestedLines = journalLines
                    .Where(a => a.Debit > 0 || a.Credit > 0)
                    .ToList();
                if (requestedLines.Count > 0)
                {
                    var journalCodes = requestedLines.Where(a => a.AccountId is null).Select(a => a.AccountCode).Distinct().ToList();
                    var journalAccountEntities = await _context.Accounts
                        .Where(a => journalCodes.Contains(a.Code))
                        .ToListAsync();
                    if (journalAccountEntities.Count != journalCodes.Count)
                    {
                        await _journals.EnsureDefaultAccountsAsync();
                        journalAccountEntities = await _context.Accounts
                            .Where(a => journalCodes.Contains(a.Code))
                            .ToListAsync();
                    }

                    var journalAccountsByCode = journalAccountEntities.ToDictionary(a => a.Code, a => a);
                    var journalAccountIdsByCode = journalAccountEntities.ToDictionary(a => a.Code, a => a.Id);

                    await _context.EntryMasters.AddAsync(new EntryMaster
                    {
                        Code = bill.Code,
                        EntryDate = bill.Date,
                        TotalBalance = requestedLines.Sum(a => Math.Round(a.Debit, 2)),
                        CurrencyRate = 1,
                        IsPosted = true,
                        EntryRemarks = $"{(LanguageHelper.IsArabic() ? entrySetting.ArName : entrySetting.EnName)} {bill.Code}",
                        AccountBoxOrBankId = paid > 0 && journalAccountIdsByCode.TryGetValue(AccountCodes.Cash, out var cashId) ? cashId : null,
                        Bill = bill,
                        EntrySetting = entrySetting,
                        CreatedAt = DateTime.Now,
                        EntryDetails = requestedLines.Select((line, index) => new EntryDetail
                        {
                            EntryRowCode = (short)(index + 1),
                            AccountId = line.AccountId ?? journalAccountIdsByCode[line.AccountCode],
                            Debit = Math.Round(line.Debit, 2),
                            Credit = Math.Round(line.Credit, 2),
                            EntryDetailsRemarks = line.Description,
                            CreatedAt = DateTime.Now
                        }).ToList()
                    });

                    // the partner (customer/supplier) side of each line is already adjusted directly above;
                    // only the system/GL accounts resolved here by AccountCode need their stored balance kept in sync with this posting.
                    foreach (var line in requestedLines.Where(a => a.AccountId is null))
                    {
                        journalAccountsByCode[line.AccountCode].Adjust(line.Debit - line.Credit);
                    }
                }
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
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
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
                return new ResultMessage { Success = false, Message = "BillNotEditable" };
            }

            var deleteType = BillSupport.TypeOf(bill.BillSetting.BillType);
            var deleteSign = -(deleteType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
            var deleteStoreId = bill.StoreId ?? 0;

            foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
            {
                var deleteProductId = BillSupport.ProductIdOf(detail.Item?.Code);
                if (deleteProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                {
                    continue;
                }

                await StoreStock.AdjustAsync(_context, deleteStoreId, deleteProductId, deleteSign * (detail.Quantity ?? 0), allowNegative: true);
                await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                {
                    StoreId = deleteStoreId,
                    ProductId = deleteProductId,
                    QuantityDelta = deleteSign * (detail.Quantity ?? 0),
                    Source = StockMovementSource.Invoice,
                    ReferenceCode = bill.Code
                });
            }

            if (bill.IsEntryPosted)
            {
                var deleteBalanceDelta = -(deleteType is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1) * bill.CasherRemainingAmount;
                if (bill.CustomerOrSupplierAccountId.HasValue)
                {
                    var deleteAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == bill.CustomerOrSupplierAccountId.Value);
                    deleteAccount?.Adjust(deleteBalanceDelta);
                }
            }

            var negative = _context.ChangeTracker.Entries<StoreItemUnit>().Any(a => a.Entity.Quantity < 0);
            if (negative)
            {
                _context.ChangeTracker.Clear();
                return new ResultMessage { Success = false, Message = "InsufficientStock" };
            }

            var entriesToRemove = await _context.EntryMasters
                .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
                .Where(a => a.BillId == bill.Id)
                .ToListAsync();

            foreach (var detailToRemove in entriesToRemove.SelectMany(a => a.EntryDetails))
            {
                if (detailToRemove.Account is not null && detailToRemove.Account.AccountType is not (AccountType.Customer or AccountType.Supplier))
                {
                    detailToRemove.Account.Adjust(-(detailToRemove.Debit - detailToRemove.Credit));
                }
            }

            _context.EntryDetails.RemoveRange(entriesToRemove.SelectMany(a => a.EntryDetails));
            _context.EntryMasters.RemoveRange(entriesToRemove);

            _context.BillDetails.RemoveRange(bill.BillDetails);
            _context.BillMasters.Remove(bill);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ResultMessage { Success = false, Message = "ConcurrentUpdate" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    private sealed record Partner(int? SupplierId, int? CustomerId, int? AccountId);

    public async Task<bool> IsInvoiceAsync(int id)
    {
        var billTypes = BillSupport.InvoiceBillTypes;

        return await _context.BillMasters.AnyAsync(a => a.Id == id && billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id));
    }

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
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (bill.IsEntryPosted && bill.IsStorageEffected)
            {
                return new ResultMessage { Success = false, Message = "BillAlreadyPosted" };
            }

            var type = BillSupport.TypeOf(bill.BillSetting.BillType);
            var remaining = bill.CasherRemainingAmount;

            if (!bill.IsEntryPosted)
            {
                var typeInfo = BillSupport.Types[type];
                var arName = typeInfo.ArName;
                var enName = typeInfo.EnName;
                var entryType = typeInfo.Entry;
                var entrySetting = await BillSupport.GetEntrySettingAsync(_context, entryType, arName, enName);

                var journalLinesBase = type switch
                {
                    InvoiceType.Purchases => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, bill.TotalBillAmountAfterDiscountAndExtra, 0),
                        new JournalLineRequest(AccountCodes.AccountsPayable, 0, remaining, null, bill.CustomerOrSupplierAccountId),
                        new JournalLineRequest(AccountCodes.Cash, 0, bill.AmountPaided)
                    },
                    InvoiceType.Returns => new[]
                    {
                        new JournalLineRequest(AccountCodes.AccountsPayable, remaining, 0, null, bill.CustomerOrSupplierAccountId),
                        new JournalLineRequest(AccountCodes.Cash, bill.AmountPaided, 0),
                        new JournalLineRequest(AccountCodes.PurchaseReturns, 0, bill.TotalBillAmountAfterDiscountAndExtra)
                    },
                    InvoiceType.Sales => new[]
                    {
                        new JournalLineRequest(AccountCodes.AccountsReceivable, remaining, 0, null, bill.CustomerOrSupplierAccountId),
                        new JournalLineRequest(AccountCodes.Cash, bill.AmountPaided, 0),
                        new JournalLineRequest(AccountCodes.SalesRevenue, 0, bill.TotalBillAmountAfterDiscountAndExtra)
                    },
                    InvoiceType.SalesReturn => new[]
                    {
                        new JournalLineRequest(AccountCodes.SalesReturns, bill.TotalBillAmountAfterDiscountAndExtra, 0),
                        new JournalLineRequest(AccountCodes.AccountsReceivable, 0, remaining, null, bill.CustomerOrSupplierAccountId),
                        new JournalLineRequest(AccountCodes.Cash, 0, bill.AmountPaided)
                    },
                    InvoiceType.Shortage => new[]
                    {
                        new JournalLineRequest(AccountCodes.InventoryAdjustment, bill.TotalBillAmountAfterDiscountAndExtra, 0),
                        new JournalLineRequest(AccountCodes.Inventory, 0, bill.TotalBillAmountAfterDiscountAndExtra)
                    },
                    InvoiceType.Surplus => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, bill.TotalBillAmountAfterDiscountAndExtra, 0),
                        new JournalLineRequest(AccountCodes.InventoryAdjustment, 0, bill.TotalBillAmountAfterDiscountAndExtra)
                    },
                    InvoiceType.OpeningInventory => new[]
                    {
                        new JournalLineRequest(AccountCodes.Inventory, bill.TotalBillAmountAfterDiscountAndExtra, 0),
                        new JournalLineRequest(AccountCodes.OpeningBalanceEquity, 0, bill.TotalBillAmountAfterDiscountAndExtra)
                    },
                    _ => Array.Empty<JournalLineRequest>()
                };

                if (type == InvoiceType.Sales && bill.BillCost is > 0)
                {
                    journalLinesBase = journalLinesBase.Concat(new[]
                    {
                        new JournalLineRequest(AccountCodes.Purchases, bill.BillCost.Value, 0),
                        new JournalLineRequest(AccountCodes.Inventory, 0, bill.BillCost.Value)
                    }).ToArray();
                }

                var journalLines = journalLinesBase.ToList();
                var posting = await _journals.PostSourceEntryAsync(JournalSourceType.Invoice, 0, bill.Date, bill.Code, journalLines, saveChanges: false);
                if (!posting.Success)
                {
                    return posting;
                }

                var requestedLines = journalLines
                    .Where(a => a.Debit > 0 || a.Credit > 0)
                    .ToList();
                if (requestedLines.Count > 0)
                {
                    var journalCodes = requestedLines.Where(a => a.AccountId is null).Select(a => a.AccountCode).Distinct().ToList();
                    var journalAccountEntities = await _context.Accounts
                        .Where(a => journalCodes.Contains(a.Code))
                        .ToListAsync();
                    if (journalAccountEntities.Count != journalCodes.Count)
                    {
                        await _journals.EnsureDefaultAccountsAsync();
                        journalAccountEntities = await _context.Accounts
                            .Where(a => journalCodes.Contains(a.Code))
                            .ToListAsync();
                    }

                    var journalAccountsByCode = journalAccountEntities.ToDictionary(a => a.Code, a => a);
                    var journalAccountIdsByCode = journalAccountEntities.ToDictionary(a => a.Code, a => a.Id);

                    await _context.EntryMasters.AddAsync(new EntryMaster
                    {
                        Code = bill.Code,
                        EntryDate = bill.Date,
                        TotalBalance = requestedLines.Sum(a => Math.Round(a.Debit, 2)),
                        CurrencyRate = 1,
                        IsPosted = true,
                        EntryRemarks = $"{(LanguageHelper.IsArabic() ? entrySetting.ArName : entrySetting.EnName)} {bill.Code}",
                        AccountBoxOrBankId = bill.AmountPaided > 0 && journalAccountIdsByCode.TryGetValue(AccountCodes.Cash, out var cashId) ? cashId : null,
                        Bill = bill,
                        EntrySetting = entrySetting,
                        CreatedAt = DateTime.Now,
                        EntryDetails = requestedLines.Select((line, index) => new EntryDetail
                        {
                            EntryRowCode = (short)(index + 1),
                            AccountId = line.AccountId ?? journalAccountIdsByCode[line.AccountCode],
                            Debit = Math.Round(line.Debit, 2),
                            Credit = Math.Round(line.Credit, 2),
                            EntryDetailsRemarks = line.Description,
                            CreatedAt = DateTime.Now
                        }).ToList()
                    });

                    foreach (var line in requestedLines.Where(a => a.AccountId is null))
                    {
                        journalAccountsByCode[line.AccountCode].Adjust(line.Debit - line.Credit);
                    }
                }

                if (bill.CustomerOrSupplierAccountId.HasValue)
                {
                    var postBalanceDelta = (type is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1) * remaining;
                    var postAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == bill.CustomerOrSupplierAccountId.Value);
                    postAccount?.Adjust(postBalanceDelta);
                }

                bill.IsEntryPosted = true;
            }

            if (!bill.IsStorageEffected)
            {
                var sign = (type is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
                foreach (var detail in bill.BillDetails)
                {
                    var productId = BillSupport.ProductIdOf(detail.Item?.Code);
                    if (productId == 0)
                    {
                        continue;
                    }

                    await StoreStock.AdjustAsync(_context, bill.StoreId ?? 0, productId, sign * (detail.Quantity ?? 0), allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = bill.StoreId ?? 0,
                        ProductId = productId,
                        QuantityDelta = sign * (detail.Quantity ?? 0),
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
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
                    return new ResultMessage { Success = false, Message = "InsufficientStock" };
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
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }


}
