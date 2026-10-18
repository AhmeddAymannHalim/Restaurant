using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class BillRepository : IBillRepository
{
    private readonly ApplicationDbContext _context;

    public BillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<BillListItemViewModel>> GetPagedBillsAsync(PageRequest request)
    {
        var invoiceTypes = BillSupport.InvoiceBillTypes;
        var query = _context.BillMasters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.OfKind(_context, request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Code.ToLower().Contains(search)
                || a.CustomerOrSupplierAccount.ArName.ToLower().Contains(search)
                || a.CustomerOrSupplierAccount.EnName.ToLower().Contains(search)
                || _context.Orders.Any(p => p.Id == a.Id && p.CustomerName.ToLower().Contains(search)));
        }

        if (request.UnpaidOnly)
        {
            query = query.Where(a => a.CasherRemainingAmount != 0 && a.CustomerOrSupplierAccountId != null);
        }

        if (request.FromDate.HasValue && !request.UnpaidOnly)
        {
            query = query.Where(a => a.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue && !request.UnpaidOnly)
        {
            query = query.Where(a => a.Date < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var rawRows = await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.Date,
                IsCashier = _context.Orders.Any(p => p.Id == a.Id),
                BillType = a.BillSetting.BillType,
                SettingArName = a.BillSetting.ArName,
                SettingEnName = a.BillSetting.EnName,
                PartnerArNameRaw = a.CustomerOrSupplierAccount.ArName,
                PartnerEnNameRaw = a.CustomerOrSupplierAccount.EnName,
                FallbackCustomerName = _context.Orders.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                Total = a.TotalBillAmountAfterDiscountAndExtra,
                Paid = a.AmountPaided,
                Remaining = a.CasherRemainingAmount,
                a.IsEntryPosted,
                a.IsStorageEffected,
                HasEntryMaster = _context.EntryMasters.Any(e => e.BillId == a.Id),
                IsCashAndCollected = _context.Orders.Any(p => p.Id == a.Id && p.Cash)
            })
            .ToListAsync();

        var data = new List<BillListItemViewModel>();
        foreach (var row in rawRows)
        {
            string settingCode;
            if (row.IsCashier)
            {
                settingCode = "ORD";
            }
            else if (row.BillType == BillType.Purchases)
            {
                settingCode = "PUR";
            }
            else if (row.BillType == BillType.PurchaseReturn)
            {
                settingCode = "PRET";
            }
            else if (row.BillType == BillType.Sales)
            {
                settingCode = "SAL";
            }
            else if (row.BillType == BillType.SalesReturn)
            {
                settingCode = "SRET";
            }
            else
            {
                settingCode = "OTHER";
            }

            var partnerArName = row.PartnerArNameRaw;
            if (partnerArName is null)
            {
                partnerArName = row.FallbackCustomerName;
            }

            var partnerEnName = row.PartnerEnNameRaw;
            if (partnerEnName is null)
            {
                partnerEnName = row.FallbackCustomerName;
            }

            var isInvoiceType = invoiceTypes.Contains(row.BillType);
            var isEditable = isInvoiceType && !row.IsCashier;

            var canPostAsInvoice = isInvoiceType && !row.IsCashier && (!row.IsEntryPosted || !row.IsStorageEffected);
            var canPostAsCashOrder = !row.HasEntryMaster && row.IsCashAndCollected;
            var canPost = canPostAsInvoice || canPostAsCashOrder;

            data.Add(new BillListItemViewModel
            {
                Id = row.Id,
                Code = row.Code,
                Date = row.Date,
                SettingCode = settingCode,
                SettingArName = row.SettingArName,
                SettingEnName = row.SettingEnName,
                PartnerArName = partnerArName,
                PartnerEnName = partnerEnName,
                Total = row.Total,
                Paid = row.Paid,
                Remaining = row.Remaining,
                IsEntryPosted = row.IsEntryPosted,
                IsCashier = row.IsCashier,
                IsEditable = isEditable,
                CanPost = canPost
            });
        }

        return PaginatedResult<BillListItemViewModel>.Create(data, total, request);
    }

    public async Task<BillViewModel> GetBillByIdAsync(int id)
    {
        var invoiceTypes = BillSupport.InvoiceBillTypes;
        var rawBill = await _context.BillMasters.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.Date,
                IsCashier = _context.Orders.Any(p => p.Id == a.Id),
                BillType = a.BillSetting.BillType,
                SettingArName = a.BillSetting.ArName,
                SettingEnName = a.BillSetting.EnName,
                PartnerArNameRaw = a.CustomerOrSupplierAccount.ArName,
                PartnerEnNameRaw = a.CustomerOrSupplierAccount.EnName,
                FallbackCustomerName = _context.Orders.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                a.TotalAmountBeforeDiscountOrExtra,
                a.TotalDiscountValueOnBillAndItems,
                a.TotalExtraValueOnBillAndItems,
                Total = a.TotalBillAmountAfterDiscountAndExtra,
                Paid = a.AmountPaided,
                Remaining = a.CasherRemainingAmount,
                a.BillCost,
                a.BillProfit,
                a.IsEntryPosted,
                a.IsStorageEffected,
                HasEntryMaster = _context.EntryMasters.Any(e => e.BillId == a.Id),
                IsCashAndCollected = _context.Orders.Any(p => p.Id == a.Id && p.Cash),
                Lines = a.BillDetails
                    .OrderBy(d => d.RowNumber)
                    .ThenBy(d => d.Id)
                    .Select(d => new BillLineViewModel
                    {
                        Id = d.Id,
                        Row = d.RowNumber ?? 0,
                        ItemCode = d.Item.Code,
                        ItemArName = d.Item.ArName,
                        ItemEnName = d.Item.EnName,
                        UnitArName = d.Unit.ArName,
                        UnitEnName = d.Unit.EnName,
                        Quantity = d.Quantity ?? 0,
                        UnitPrice = d.UnitPrice ?? 0,
                        Total = d.TotalAmount ?? 0
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (rawBill is null)
        {
            return null;
        }

        string settingCode;
        if (rawBill.IsCashier)
        {
            settingCode = "ORD";
        }
        else if (rawBill.BillType == BillType.Purchases)
        {
            settingCode = "PUR";
        }
        else if (rawBill.BillType == BillType.PurchaseReturn)
        {
            settingCode = "PRET";
        }
        else if (rawBill.BillType == BillType.Sales)
        {
            settingCode = "SAL";
        }
        else if (rawBill.BillType == BillType.SalesReturn)
        {
            settingCode = "SRET";
        }
        else
        {
            settingCode = "OTHER";
        }

        var partnerArName = rawBill.PartnerArNameRaw;
        if (partnerArName is null)
        {
            partnerArName = rawBill.FallbackCustomerName;
        }

        var partnerEnName = rawBill.PartnerEnNameRaw;
        if (partnerEnName is null)
        {
            partnerEnName = rawBill.FallbackCustomerName;
        }

        var isInvoiceType = invoiceTypes.Contains(rawBill.BillType);
        var isEditable = isInvoiceType && !rawBill.IsCashier;

        var canPostAsInvoice = isInvoiceType && !rawBill.IsCashier && (!rawBill.IsEntryPosted || !rawBill.IsStorageEffected);
        var canPostAsCashOrder = !rawBill.HasEntryMaster && rawBill.IsCashAndCollected;
        var canPost = canPostAsInvoice || canPostAsCashOrder;

        var bill = new BillViewModel
        {
            Id = rawBill.Id,
            Code = rawBill.Code,
            Date = rawBill.Date,
            SettingCode = settingCode,
            SettingArName = rawBill.SettingArName,
            SettingEnName = rawBill.SettingEnName,
            PartnerArName = partnerArName,
            PartnerEnName = partnerEnName,
            ItemsTotal = rawBill.TotalAmountBeforeDiscountOrExtra,
            Discount = rawBill.TotalDiscountValueOnBillAndItems,
            Extras = rawBill.TotalExtraValueOnBillAndItems,
            Total = rawBill.Total,
            Paid = rawBill.Paid,
            Remaining = rawBill.Remaining,
            Cost = rawBill.BillCost,
            Profit = rawBill.BillProfit,
            IsEntryPosted = rawBill.IsEntryPosted,
            IsStorageEffected = rawBill.IsStorageEffected,
            IsCashier = rawBill.IsCashier,
            IsEditable = isEditable,
            CanPost = canPost,
            Lines = rawBill.Lines
        };

        bill.Entries = await _context.EntryMasters.AsNoTracking()
            .Where(a => a.BillId == id)
            .OrderBy(a => a.Id)
            .Select(a => new EntryViewModel
            {
                Id = a.Id,
                Code = a.Code,
                EntryDate = a.EntryDate,
                EntryType = a.EntrySetting == null ? null : a.EntrySetting.EntryType,
                SettingArName = a.EntrySetting.ArName,
                SettingEnName = a.EntrySetting.EnName,
                Total = a.TotalBalance,
                IsPosted = a.IsPosted,
                Remarks = a.EntryRemarks,
                BillId = a.BillId,
                BillCode = a.Bill.Code,
                Lines = a.EntryDetails
                    .OrderBy(d => d.EntryRowCode)
                    .Select(d => new EntryLineViewModel
                    {
                        Row = d.EntryRowCode,
                        AccountCode = d.Account.Code,
                        AccountArName = d.Account.ArName,
                        AccountEnName = d.Account.EnName,
                        Debit = d.Debit,
                        Credit = d.Credit,
                        Remarks = d.EntryDetailsRemarks
                    })
                    .ToList()
            })
            .ToListAsync();

        return bill;
    }

    public async Task<ResultMessage> PostBillAsync(int id)
    {
        try
        {
            var pos = await _context.Orders.Include(a => a.Bill).FirstOrDefaultAsync(a => a.Id == id);
            if (pos is null)
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            if (!pos.Cash)
            {
                return new ResultMessage { Success = false, Message = "BillNotCollected" };
            }

            if (await _context.EntryMasters.AnyAsync(a => a.BillId == id))
            {
                return new ResultMessage { Success = false, Message = "BillAlreadyPosted" };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _context.EntryMasters.AnyAsync(a => a.BillId == id))
                {
                    await transaction.RollbackAsync();
                    return new ResultMessage { Success = false, Message = "BillAlreadyPosted" };
                }

                await OrderLedger.PostEntryAsync(_context, pos);
                pos.Bill.IsEntryPosted = true;
                pos.Bill.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultMessage { Success = true };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> PostPendingAsync()
    {
        try
        {
            var ids = await _context.Orders.AsNoTracking()
                .Where(a => a.Cash && !_context.EntryMasters.Any(e => e.BillId == a.Id))
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .ToListAsync();

            var posted = 0;
            foreach (var id in ids)
            {
                var result = await PostBillAsync(id);
                if (!result.Success)
                {
                    return new ResultMessage { Success = false, Message = result.Message, Data = posted.ToString() };
                }

                _context.ChangeTracker.Clear();
                posted++;
            }

            return new ResultMessage { Success = true, Data = posted.ToString() };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

}
