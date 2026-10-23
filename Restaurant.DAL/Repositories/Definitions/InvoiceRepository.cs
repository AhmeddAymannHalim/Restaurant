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
    private readonly IStockRepository _stock;
    private readonly IBillSupportRepository _billSupport;
    private readonly IJournalEntryRepository _journals;

    public InvoiceRepository(ApplicationDbContext context, IJournalEntryRepository journals)
    {
        _context = context;
        _stock = new StockRepository(context);
        _billSupport = new BillSupportRepository(context);
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
        if (bill is null || !await _billSupport.IsInvoiceAsync(bill))
        {
            return null;
        }

        var type = _billSupport.TypeOf(bill.BillSetting.BillType);
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
            Paid = bill.InitialPaidAmount,
            AutoPost = bill.IsEntryPosted,
            AutoStock = bill.IsStorageEffected,
            Items = bill.BillDetails
                .OrderBy(a => a.RowNumber)
                .ThenBy(a => a.Id)
                .Select(a => new InvoiceItemViewModel
                {
                    Id = a.Id,
                    ProductId = _billSupport.ProductIdOf(a.Item?.Code),
                    ProductName = isArabic ? a.Item?.ArName : a.Item?.EnName,
                    UnitPriceId = _billSupport.UnitPriceIdOf(a.Unit?.Code),
                    UnitName = isArabic ? a.Unit?.ArName : a.Unit?.EnName,
                    Quantity = a.Quantity ?? 0,
                    Price = a.UnitPrice ?? 0
                })
                .ToList()
        };

        model.LinkedPayments = (await _context.PaymentInvoiceSupplier.Where(a => a.BillMasterId == bill.Id).SumAsync(a => (decimal?)a.Paid) ?? 0)
            + (await _context.PaymentInvoiceCustomer.Where(a => a.BillMasterId == bill.Id).SumAsync(a => (decimal?)a.Paid) ?? 0);

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

    // a blank invoice of one type: its next code and the posting choices that type starts with
    public async Task<InvoiceViewModel> GetNewInvoiceAsync(InvoiceType type)
    {
        var model = new InvoiceViewModel { InvoiceType = type };
        model.InvoiceCode = await GetNextCodeAsync(type);
        model.PostingDefaults = await GetPostingDefaultsAsync();
        var defaults = model.PostingDefaults.FirstOrDefault(a => a.Type == type);
        model.AutoPost = defaults?.AutoPost ?? true;
        model.AutoStock = defaults?.AutoStock ?? true;

        return model;
    }

    public async Task<string> GetNextCodeAsync(InvoiceType type)
    {
        var typeInfo = _billSupport.GetInvoiceType(type);
        var billType = typeInfo.BillType;
        var arName = typeInfo.ArName;
        var enName = typeInfo.EnName;
        var setting = await _billSupport.GetBillSettingAsync(billType, arName, enName);

        return await _billSupport.NextBillCodeAsync(setting.Id);
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

            InvoicePartner partner;
            if (!model.HasPartner)
            {
                partner = new InvoicePartner();
            }
            else if (model.IsSupplierInvoice)
            {
                var supplier = model.SupplierId.HasValue
                    ? await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.SupplierId.Value)
                    : null;
                partner = new InvoicePartner { SupplierId = supplier?.Id, AccountId = supplier?.AccountId };
            }
            else
            {
                var customer = model.CustomerId.HasValue
                    ? await _context.Customers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.CustomerId.Value)
                    : null;
                partner = new InvoicePartner { CustomerId = customer?.Id, AccountId = customer?.AccountId };
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

            var typeInfo = _billSupport.GetInvoiceType(model.InvoiceType);
            var billType = typeInfo.BillType;
            var arName = typeInfo.ArName;
            var enName = typeInfo.EnName;
            var entryType = typeInfo.Entry;

            var setting = await _billSupport.GetBillSettingAsync(billType, arName, enName);
            var entrySetting = await _billSupport.GetEntrySettingAsync(entryType, arName, enName);
            // the item, unit and cost of every line (same order as the lines)
            // and how many base units one unit of the line holds (1 when the product has no rate for that unit)
            var itemIds = new List<int>();
            var unitIds = new List<int>();
            var rates = new List<decimal>();
            var costs = new List<decimal>();
            foreach (var item in items)
            {
                var itemCode = BillSupportRepository.ItemCodePrefix + item.ProductId;
                var unitCode = BillSupportRepository.UnitCodePrefix + item.UnitPriceId;
                decimal rate = 1;
                var foundRate = await _context.ItemUnits.AsNoTracking()
                    .Where(a => a.Item.Code == itemCode && a.Unit.Code == unitCode)
                    .Select(a => (double?)a.TransRate)
                    .FirstOrDefaultAsync();
                if (foundRate > 0)
                {
                    rate = (decimal)foundRate.Value;
                }

                var productCost = await _context.Products.AsNoTracking().Where(a => a.Id == item.ProductId).Select(a => a.Cost).FirstOrDefaultAsync();
                itemIds.Add(await _billSupport.GetItemIdAsync(item.ProductId));
                unitIds.Add(await _billSupport.GetUnitIdAsync(item.UnitPriceId));
                rates.Add(rate);
                costs.Add(productCost * rate);
            }

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

                if (!await _billSupport.IsInvoiceAsync(bill))
                {
                    return new ResultMessage { Success = false, Message = "BillNotEditable" };
                }

                var oldType = _billSupport.TypeOf(bill.BillSetting.BillType);
                var takeBackSign = -(oldType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
                var takeBackStoreId = bill.StoreId ?? 0;

                foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
                {
                    var takeBackProductId = _billSupport.ProductIdOf(detail.Item?.Code);
                    if (takeBackProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                    {
                        continue;
                    }

                    var takeBackQuantity = takeBackSign * (detail.Quantity ?? 0) * (detail.TransRate ?? 1);
                    await _stock.AdjustAsync(takeBackStoreId, takeBackProductId, takeBackQuantity, allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = takeBackStoreId,
                        ProductId = takeBackProductId,
                        QuantityDelta = takeBackQuantity,
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
                }

                if (bill.IsEntryPosted)
                {
                    var takeBackBalanceDelta = -(oldType is InvoiceType.Purchases or InvoiceType.Sales ? 1 : -1) * (bill.TotalBillAmountAfterDiscountAndExtra - bill.InitialPaidAmount);
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
                        existingDetail.Account.Adjust(existingDetail.Account.BalanceType == AccountBalanceType.Debit ? existingDetail.Credit - existingDetail.Debit : existingDetail.Debit - existingDetail.Credit);
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
                ItemId = itemIds[index],
                UnitId = unitIds[index],
                StoreId = model.StoreId,
                Quantity = line.Quantity,
                UnitPrice = line.Price,
                TransRate = rates[index],
                TotalAmount = Math.Round(line.Quantity * line.Price, 2),
                UnitCostAtPosting = costs[index],
                CreatedAt = DateTime.Now
            }).ToList();

            var itemsTotal = details.Sum(a => a.TotalAmount ?? 0);
            var discount = model.Discount;
            var total = itemsTotal - discount;
            var paid = model.Paid;
            var remaining = total - paid;
            decimal linkedPayments = 0;
            if (model.Id != 0)
            {
                linkedPayments = (await _context.PaymentInvoiceSupplier.Where(a => a.BillMasterId == model.Id).SumAsync(a => (decimal?)a.Paid) ?? 0)
                    + (await _context.PaymentInvoiceCustomer.Where(a => a.BillMasterId == model.Id).SumAsync(a => (decimal?)a.Paid) ?? 0);
            }
            if (paid + linkedPayments > total)
            {
                return new ResultMessage { Success = false, Message = "Val.PaidTooBig" };
            }

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
            bill.AmountPaided = paid + linkedPayments;
            bill.CasherRemainingAmount = remaining - linkedPayments;
            bill.InitialPaidAmount = paid;
            bill.IsStorageEffected = model.AutoStock;
            bill.IsEntryPosted = model.AutoPost;
            bill.BillCost = cost;
            // a sales return takes back the profit of the sale
            bill.BillProfit = cost.HasValue ? (model.InvoiceType == InvoiceType.SalesReturn ? cost - total : total - cost) : null;
            bill.BillDetails = details;

            var applySign = (model.InvoiceType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
            var applyStoreId = bill.StoreId.Value;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                await _billSupport.EnsureItemUnitAsync(itemIds[i], unitIds[i]);
                if (bill.IsStorageEffected)
                {
                    var applyQuantity = applySign * item.Quantity * rates[i];
                    await _stock.AdjustAsync(applyStoreId, item.ProductId, applyQuantity, allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = applyStoreId,
                        ProductId = item.ProductId,
                        QuantityDelta = applyQuantity,
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
                }
            }

            if (model.InvoiceType == InvoiceType.Purchases && bill.IsStorageEffected)
            {
                // the product cost is per base unit, the line price is per the unit of the line
                for (var i = 0; i < items.Count; i++)
                {
                    var productToUpdateCost = await _context.Products.FindAsync(items[i].ProductId);
                    if (productToUpdateCost is not null)
                    {
                        productToUpdateCost.Cost = items[i].Price / rates[i];
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
                await _journals.PostBillEntryAsync(bill, model.InvoiceType, paid, remaining, partner.AccountId, entrySetting);
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

            if (!await _billSupport.IsInvoiceAsync(bill))
            {
                return new ResultMessage { Success = false, Message = "BillNotEditable" };
            }

            var billPayments = (await _context.PaymentInvoiceSupplier.Where(a => a.BillMasterId == bill.Id).SumAsync(a => (decimal?)a.Paid) ?? 0)
                + (await _context.PaymentInvoiceCustomer.Where(a => a.BillMasterId == bill.Id).SumAsync(a => (decimal?)a.Paid) ?? 0);
            if (billPayments > 0)
            {
                return new ResultMessage { Success = false, Message = "InvoiceHasPayments" };
            }

            var deleteType = _billSupport.TypeOf(bill.BillSetting.BillType);
            var deleteSign = -(deleteType is InvoiceType.Purchases or InvoiceType.SalesReturn or InvoiceType.Surplus or InvoiceType.OpeningInventory ? 1 : -1);
            var deleteStoreId = bill.StoreId ?? 0;

            foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
            {
                var deleteProductId = _billSupport.ProductIdOf(detail.Item?.Code);
                if (deleteProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                {
                    continue;
                }

                var deleteQuantity = deleteSign * (detail.Quantity ?? 0) * (detail.TransRate ?? 1);
                await _stock.AdjustAsync(deleteStoreId, deleteProductId, deleteQuantity, allowNegative: true);
                await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                {
                    StoreId = deleteStoreId,
                    ProductId = deleteProductId,
                    QuantityDelta = deleteQuantity,
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
                    detailToRemove.Account.Adjust(detailToRemove.Account.BalanceType == AccountBalanceType.Debit ? detailToRemove.Credit - detailToRemove.Debit : detailToRemove.Debit - detailToRemove.Credit);
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

    public async Task<bool> IsInvoiceAsync(int id)
    {
        var billTypes = _billSupport.GetInvoiceBillTypes();

        return await _context.BillMasters.AnyAsync(a => a.Id == id && billTypes.Contains(a.BillSetting.BillType) && !_context.Orders.Any(p => p.Id == a.Id));
    }

    public async Task<List<PostingDefaultViewModel>> GetPostingDefaultsAsync()
    {
        var invoiceBillTypes = _billSupport.GetInvoiceBillTypes();
        var settings = await _context.BillSettings.AsNoTracking()
            .Where(a => invoiceBillTypes.Contains(a.BillType))
            .OrderBy(a => a.Id)
            .Select(a => new { a.BillType, a.EntryGenerate, a.StorageEffect })
            .ToListAsync();

        var defaults = new List<PostingDefaultViewModel>();
        foreach (var info in _billSupport.GetAllInvoiceTypes())
        {
            var setting = settings.FirstOrDefault(a => a.BillType == info.BillType);

            defaults.Add(new PostingDefaultViewModel
            {
                Type = info.Type,
                AutoPost = setting is null || setting.EntryGenerate == EntryGenerate.AutomaticEntryCreated,
                AutoStock = setting is null || setting.StorageEffect == StorageEffect.AutoEffected
            });
        }

        return defaults;
    }

    public async Task<List<int>> GetUnpostedInvoiceIdsAsync()
    {
        var billTypes = _billSupport.GetInvoiceBillTypes();

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
            if (bill is null || !await _billSupport.IsInvoiceAsync(bill))
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (bill.IsEntryPosted && bill.IsStorageEffected)
            {
                return new ResultMessage { Success = false, Message = "BillAlreadyPosted" };
            }

            var type = _billSupport.TypeOf(bill.BillSetting.BillType);
            var paidAtCreation = bill.InitialPaidAmount;
            var remaining = bill.TotalBillAmountAfterDiscountAndExtra - paidAtCreation;

            if (!bill.IsEntryPosted)
            {
                var typeInfo = _billSupport.GetInvoiceType(type);
                var arName = typeInfo.ArName;
                var enName = typeInfo.EnName;
                var entryType = typeInfo.Entry;
                var entrySetting = await _billSupport.GetEntrySettingAsync(entryType, arName, enName);

                await _journals.PostBillEntryAsync(bill, type, paidAtCreation, remaining, bill.CustomerOrSupplierAccountId, entrySetting);

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
                    var productId = _billSupport.ProductIdOf(detail.Item?.Code);
                    if (productId == 0)
                    {
                        continue;
                    }

                    var postRate = detail.TransRate ?? 1;
                    var postQuantity = sign * (detail.Quantity ?? 0) * postRate;
                    await _stock.AdjustAsync(bill.StoreId ?? 0, productId, postQuantity, allowNegative: true);
                    await _context.StoreProductMovements.AddAsync(new StoreProductMovement
                    {
                        StoreId = bill.StoreId ?? 0,
                        ProductId = productId,
                        QuantityDelta = postQuantity,
                        Source = StockMovementSource.Invoice,
                        ReferenceCode = bill.Code
                    });
                    if (type == InvoiceType.Purchases && detail.UnitPrice.HasValue)
                    {
                        var product = await _context.Products.FindAsync(productId);
                        if (product is not null)
                        {
                            product.Cost = detail.UnitPrice.Value / (postRate > 0 ? postRate : 1);
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
