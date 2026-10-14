using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ErpBillRepository : IErpBillRepository
{
    private readonly ApplicationDbContext _context;

    public ErpBillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<ErpBillListItemViewModel>> GetPagedBillsAsync(PageRequest request)
    {
        var query = _context.BillMasters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(a => a.BillSetting.Code == request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Code.ToLower().Contains(search)
                || a.CustomerOrSupplierAccount.ArName.ToLower().Contains(search)
                || a.CustomerOrSupplierAccount.EnName.ToLower().Contains(search)
                || _context.PosBills.Any(p => p.Id == a.Id && p.CustomerName.ToLower().Contains(search)));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Date >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Date < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(a => new ErpBillListItemViewModel
            {
                Id = a.Id,
                Code = a.Code,
                Date = a.Date,
                SettingCode = a.BillSetting.Code,
                SettingArName = a.BillSetting.ArName,
                SettingEnName = a.BillSetting.EnName,
                PartnerArName = a.CustomerOrSupplierAccount.ArName
                    ?? _context.PosBills.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                PartnerEnName = a.CustomerOrSupplierAccount.EnName
                    ?? _context.PosBills.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                Total = a.TotalBillAmountAfterDiscountAndExtra,
                Paid = a.AmountPaided,
                Remaining = a.CasherRemainingAmount,
                IsEntryPosted = a.IsEntryPosted,
                IsCashier = _context.PosBills.Any(p => p.Id == a.Id),
                IsEditable = a.BillSetting.Code == "PUR" || a.BillSetting.Code == "PRET" || a.BillSetting.Code == "SAL" || a.BillSetting.Code == "SRET",
                CanPost = !_context.EntryMasters.Any(e => e.BillId == a.Id)
                    && _context.PosBills.Any(p => p.Id == a.Id && p.Cash)
            })
            .ToListAsync();

        return PaginatedResult<ErpBillListItemViewModel>.Create(data, total, request);
    }

    public async Task<ErpBillViewModel> GetBillByIdAsync(int id)
    {
        var bill = await _context.BillMasters.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ErpBillViewModel
            {
                Id = a.Id,
                Code = a.Code,
                Date = a.Date,
                SettingCode = a.BillSetting.Code,
                SettingArName = a.BillSetting.ArName,
                SettingEnName = a.BillSetting.EnName,
                PartnerArName = a.CustomerOrSupplierAccount.ArName
                    ?? _context.PosBills.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                PartnerEnName = a.CustomerOrSupplierAccount.EnName
                    ?? _context.PosBills.Where(p => p.Id == a.Id).Select(p => p.CustomerName).FirstOrDefault(),
                ItemsTotal = a.TotalAmountBeforeDiscountOrExtra,
                Discount = a.TotalDiscountValueOnBillAndItems,
                Extras = a.TotalExtraValueOnBillAndItems,
                Total = a.TotalBillAmountAfterDiscountAndExtra,
                Paid = a.AmountPaided,
                Remaining = a.CasherRemainingAmount,
                Cost = a.BillCost,
                Profit = a.BillProfit,
                IsEntryPosted = a.IsEntryPosted,
                IsStorageEffected = a.IsStorageEffected,
                IsCashier = _context.PosBills.Any(p => p.Id == a.Id),
                IsEditable = a.BillSetting.Code == "PUR" || a.BillSetting.Code == "PRET" || a.BillSetting.Code == "SAL" || a.BillSetting.Code == "SRET",
                CanPost = !_context.EntryMasters.Any(e => e.BillId == a.Id)
                    && _context.PosBills.Any(p => p.Id == a.Id && p.Cash),
                Lines = a.BillDetails
                    .OrderBy(d => d.RowNumber)
                    .ThenBy(d => d.Id)
                    .Select(d => new ErpBillLineViewModel
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

        if (bill is null)
        {
            return null;
        }

        bill.Entries = await _context.EntryMasters.AsNoTracking()
            .Where(a => a.BillId == id)
            .OrderBy(a => a.Id)
            .Select(a => new ErpEntryViewModel
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
                    .Select(d => new ErpEntryLineViewModel
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
            var pos = await _context.PosBills.Include(a => a.Bill).FirstOrDefaultAsync(a => a.Id == id);
            if (pos is null)
            {
                return Fail("RecordNotFound");
            }

            if (!pos.Cash)
            {
                return Fail("BillNotCollected");
            }

            if (await _context.EntryMasters.AnyAsync(a => a.BillId == id))
            {
                return Fail("BillAlreadyPosted");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await PosBillLedger.PostEntryAsync(_context, pos);
            pos.Bill.IsEntryPosted = true;
            pos.Bill.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    public async Task<ResultMessage> PostPendingAsync()
    {
        try
        {
            var ids = await _context.PosBills.AsNoTracking()
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
            return Fail(ErrorLog.Message(ex));
        }
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
