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
            .Include(a => a.PaymentMethods)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (bill is null || !await _billSupport.IsInvoiceAsync(bill))
        {
            return null;
        }

        var type = _billSupport.TypeOf(bill.BillSetting.BillType);
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
            Paid = bill.PaymentMethods.Sum(a => a.AmountPaided),
            AutoPost = bill.IsEntryPosted,
            AutoStock = bill.IsStorageEffected,
            Items = bill.BillDetails
                .OrderBy(a => a.RowNumber)
                .ThenBy(a => a.Id)
                .Select(a => new InvoiceItemViewModel
                {
                    Id = a.Id,
                    ProductId = _billSupport.ProductIdOf(a.Item?.Code),
                    ProductArName = a.Item?.ArName,
                    ProductEnName = a.Item?.EnName,
                    UnitPriceId = _billSupport.UnitPriceIdOf(a.Unit?.Code),
                    UnitArName = a.Unit?.ArName,
                    UnitEnName = a.Unit?.EnName,
                    Quantity = a.Quantity ?? 0,
                    Price = a.UnitPrice ?? 0
                })
                .ToList()
        };

        model.LinkedPayments = await _billSupport.GetLinkedPaymentsAsync(bill.Id);

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
        var setting = await _billSupport.GetBillSettingAsync(typeInfo.BillType, typeInfo.ArName, typeInfo.EnName);

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
            var setting = await _billSupport.GetBillSettingAsync(typeInfo.BillType, typeInfo.ArName, typeInfo.EnName);
            var entrySetting = await _billSupport.GetEntrySettingAsync(typeInfo.Entry, typeInfo.ArName, typeInfo.EnName);
            // one BillDetail per line: stock and cost are kept per base unit, so the line remembers how many base units its unit holds
            // (1 when the product has no rate for that unit)
            var itemCodes = productIds.Select(a => BillSupportRepository.ItemCodePrefix + a).ToList();
            var unitRates = await _context.ItemUnits.AsNoTracking()
                .Include(a => a.Item)
                .Include(a => a.Unit)
                .Where(a => itemCodes.Contains(a.Item.Code))
                .ToListAsync();
            var products = await _context.Products.AsNoTracking().Where(a => productIds.Contains(a.Id)).ToListAsync();

            var details = new List<BillDetail>();
            for (var i = 0; i < items.Count; i++)
            {
                var line = items[i];
                var itemCode = BillSupportRepository.ItemCodePrefix + line.ProductId;
                var unitCode = BillSupportRepository.UnitCodePrefix + line.UnitPriceId;
                var foundRate = unitRates.FirstOrDefault(a => a.Item.Code == itemCode && a.Unit.Code == unitCode)?.TransRate;

                var rate = foundRate > 0 ? (decimal)foundRate.Value : 1;
                var productCost = products.First(a => a.Id == line.ProductId).Cost;

                details.Add(new BillDetail
                {
                    RowNumber = (byte)Math.Min(i + 1, byte.MaxValue),
                    ItemId = await _billSupport.GetItemIdAsync(line.ProductId),
                    UnitId = await _billSupport.GetUnitIdAsync(line.UnitPriceId),
                    StoreId = model.StoreId,
                    Quantity = line.Quantity,
                    UnitPrice = line.Price,
                    TransRate = rate,
                    TotalAmount = Math.Round(line.Quantity * line.Price, 2),
                    UnitCostAtPosting = productCost * rate,
                    CreatedAt = DateTime.Now
                });
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
                    .Include(a => a.PaymentMethods)
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
                var takeBackSign = -_billSupport.StockSignOf(oldType);
                var takeBackStoreId = bill.StoreId ?? 0;

                foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
                {
                    var takeBackProductId = _billSupport.ProductIdOf(detail.Item?.Code);
                    if (takeBackProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                    {
                        continue;
                    }

                    var takeBackQuantity = takeBackSign * (detail.Quantity ?? 0) * (detail.TransRate ?? 1);
                    await _stock.MoveAsync(takeBackStoreId, takeBackProductId, takeBackQuantity, StockMovementSource.Invoice, bill.Code);
                }

                if (bill.IsEntryPosted)
                {
                    var takeBackBalanceDelta = -_billSupport.BalanceSignOf(oldType) * (bill.TotalBillAmountAfterDiscountAndExtra - bill.PaymentMethods.Sum(a => a.AmountPaided));
                    if (bill.CustomerOrSupplierAccountId.HasValue)
                    {
                        var takeBackAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == bill.CustomerOrSupplierAccountId.Value);
                        takeBackAccount?.Adjust(takeBackBalanceDelta);
                    }
                }

                await _journals.RemoveBillEntriesAsync(bill.Id);

                _context.BillDetails.RemoveRange(bill.BillDetails);
                _context.PaymentMethods.RemoveRange(bill.PaymentMethods);

                bill.BillSettingId = setting.Id;
                bill.UpdatedAt = DateTime.Now;
            }

            var itemsTotal = details.Sum(a => a.TotalAmount ?? 0);
            var discount = model.Discount;
            var total = itemsTotal - discount;
            var paid = model.Paid;
            decimal linkedPayments = 0;
            if (model.Id != 0)
            {
                linkedPayments = await _billSupport.GetLinkedPaymentsAsync(model.Id);
            }
            var isCash = model.HasPartner && model.PayType == Shared.Enums.PayType.Cash;
            if (isCash)
            {
                paid = Math.Max(0, total - linkedPayments);
            }
            var remaining = total - paid;
            if (paid + linkedPayments > total)
            {
                _context.ChangeTracker.Clear();
                return new ResultMessage { Success = false, Message = "Val.PaidTooBig" };
            }

            var isSale = model.InvoiceType is InvoiceType.Sales or InvoiceType.SalesReturn;

            decimal? cost = null;
            if (isSale)
            {
                cost = details.Sum(a => (a.UnitCostAtPosting ?? 0) * (a.Quantity ?? 0));
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
            bill.PayType = isCash || !model.HasPartner ? Shared.Enums.PayType.Cash : Shared.Enums.PayType.Later;
            bill.AmountPaided = paid + linkedPayments;
            bill.CasherRemainingAmount = remaining - linkedPayments;
            bill.PaymentMethods = paid > 0
                ? new List<Restaurant.DAL.Models.PaymentMethod> { new() { RowNumber = 1, AccountPayId = setting.AccountPayId, AmountPaided = paid, CreatedAt = DateTime.Now } }
                : new List<Restaurant.DAL.Models.PaymentMethod>();
            bill.IsStorageEffected = model.AutoStock;
            bill.IsEntryPosted = model.AutoPost;
            bill.BillCost = cost;
            // a sales return takes back the profit of the sale
            bill.BillProfit = cost.HasValue ? (model.InvoiceType == InvoiceType.SalesReturn ? cost - total : total - cost) : null;
            bill.BillDetails = details;

            var applySign = _billSupport.StockSignOf(model.InvoiceType);
            var applyStoreId = bill.StoreId.Value;

            for (var i = 0; i < items.Count; i++)
            {
                await _billSupport.EnsureItemUnitAsync(details[i].ItemId.Value, details[i].UnitId.Value);
                if (bill.IsStorageEffected)
                {
                    var applyQuantity = applySign * items[i].Quantity * details[i].TransRate.Value;
                    await _stock.MoveAsync(applyStoreId, items[i].ProductId, applyQuantity, StockMovementSource.Invoice, bill.Code);
                }
            }

            if (model.InvoiceType == InvoiceType.Purchases && bill.IsStorageEffected)
            {
                foreach (var productId in items.Select(a => a.ProductId).Distinct())
                {
                    await _stock.ApplyPurchaseCostAsync(productId, details.Where((detail, i) => items[i].ProductId == productId).ToList());
                }
            }

            if (bill.IsEntryPosted && partner.AccountId.HasValue)
            {
                var applyBalanceDelta = _billSupport.BalanceSignOf(model.InvoiceType) * remaining;
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
            await _billSupport.LinkEntryAsync(bill);
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
                .Include(a => a.PaymentMethods)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (bill is null)
            {
                return new ResultMessage { Success = true };
            }

            if (!await _billSupport.IsInvoiceAsync(bill))
            {
                return new ResultMessage { Success = false, Message = "BillNotEditable" };
            }

            var hasPayments = await _context.PaymentInvoiceSupplier.AnyAsync(a => a.BillMasterId == bill.Id)
                || await _context.PaymentInvoiceCustomer.AnyAsync(a => a.BillMasterId == bill.Id);
            if (hasPayments)
            {
                return new ResultMessage { Success = false, Message = "InvoiceHasPayments" };
            }

            var deleteType = _billSupport.TypeOf(bill.BillSetting.BillType);
            var deleteSign = -_billSupport.StockSignOf(deleteType);
            var deleteStoreId = bill.StoreId ?? 0;

            foreach (var detail in bill.BillDetails.Where(_ => bill.IsStorageEffected))
            {
                var deleteProductId = _billSupport.ProductIdOf(detail.Item?.Code);
                if (deleteProductId == 0 || detail.ItemId is null || detail.UnitId is null)
                {
                    continue;
                }

                var deleteQuantity = deleteSign * (detail.Quantity ?? 0) * (detail.TransRate ?? 1);
                await _stock.MoveAsync(deleteStoreId, deleteProductId, deleteQuantity, StockMovementSource.Invoice, bill.Code);
            }

            if (bill.IsEntryPosted)
            {
                var deleteBalanceDelta = -_billSupport.BalanceSignOf(deleteType) * bill.CasherRemainingAmount;
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

            await _journals.RemoveBillEntriesAsync(bill.Id);

            _context.BillDetails.RemoveRange(bill.BillDetails);
            _context.PaymentMethods.RemoveRange(bill.PaymentMethods);
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
                .Include(a => a.PaymentMethods)
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
            var paidAtCreation = bill.PaymentMethods.Sum(a => a.AmountPaided);
            var remaining = bill.TotalBillAmountAfterDiscountAndExtra - paidAtCreation;

            if (!bill.IsEntryPosted)
            {
                var typeInfo = _billSupport.GetInvoiceType(type);
                var entrySetting = await _billSupport.GetEntrySettingAsync(typeInfo.Entry, typeInfo.ArName, typeInfo.EnName);

                await _journals.PostBillEntryAsync(bill, type, paidAtCreation, remaining, bill.CustomerOrSupplierAccountId, entrySetting);

                if (bill.CustomerOrSupplierAccountId.HasValue)
                {
                    var postBalanceDelta = _billSupport.BalanceSignOf(type) * remaining;
                    var postAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == bill.CustomerOrSupplierAccountId.Value);
                    postAccount?.Adjust(postBalanceDelta);
                }

                bill.IsEntryPosted = true;
            }

            if (!bill.IsStorageEffected)
            {
                var sign = _billSupport.StockSignOf(type);
                foreach (var detail in bill.BillDetails)
                {
                    var productId = _billSupport.ProductIdOf(detail.Item?.Code);
                    if (productId == 0)
                    {
                        continue;
                    }

                    var postRate = detail.TransRate ?? 1;
                    var postQuantity = sign * (detail.Quantity ?? 0) * postRate;
                    await _stock.MoveAsync(bill.StoreId ?? 0, productId, postQuantity, StockMovementSource.Invoice, bill.Code);
                    if (type == InvoiceType.Purchases && detail.UnitPrice.HasValue)
                    {
                        await _stock.ApplyPurchaseCostAsync(productId, new List<BillDetail> { detail });
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
            await _billSupport.LinkEntryAsync(bill);
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }


}
