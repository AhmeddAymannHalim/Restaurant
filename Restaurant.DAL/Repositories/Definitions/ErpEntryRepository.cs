using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class ErpEntryRepository : IErpEntryRepository
{
    private readonly ApplicationDbContext _context;

    public ErpEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<ErpEntryViewModel>> GetPagedEntriesAsync(PageRequest request)
    {
        var query = _context.EntryMasters.AsNoTracking().AsQueryable();

        if (Enum.TryParse<EntryType>(request.Type, out var type))
        {
            query = query.Where(a => a.EntrySetting.EntryType == type);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.Code.ToLower().Contains(search)
                || a.EntryRemarks.ToLower().Contains(search)
                || a.Bill.Code.ToLower().Contains(search));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.EntryDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.EntryDate < request.ToDate.Value.Date.AddDays(1));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.EntryDate)
            .ThenByDescending(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
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
                CollectEntryCode = a.CollectEntryCode,
                BillId = a.BillId,
                BillCode = a.Bill.Code
            })
            .ToListAsync();

        return PaginatedResult<ErpEntryViewModel>.Create(data, total, request);
    }

    public async Task<ErpEntryViewModel> GetEntryByIdAsync(int id)
    {
        var entry = await _context.EntryMasters.AsNoTracking()
            .Where(a => a.Id == id)
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
                CollectEntryCode = a.CollectEntryCode,
                BillId = a.BillId,
                BillCode = a.Bill.Code,
                IsManual = a.CollectEntryCode != null && a.CollectEntryCode.StartsWith("Manual:"),
                Lines = a.EntryDetails
                    .OrderBy(d => d.EntryRowCode)
                    .Select(d => new ErpEntryLineViewModel
                    {
                        Row = d.EntryRowCode,
                        AccountId = d.AccountId ?? 0,
                        AccountCode = d.Account.Code,
                        AccountArName = d.Account.ArName,
                        AccountEnName = d.Account.EnName,
                        Debit = d.Debit,
                        Credit = d.Credit,
                        Remarks = d.EntryDetailsRemarks
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return entry;
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        return await _context.Accounts.AsNoTracking()
            .Where(a => a.AccountType != AccountType.Collective)
            .OrderBy(a => a.Code)
            .Select(a => new LookupItem { Id = a.Id, ArName = a.Code + " - " + a.ArName, EnName = a.Code + " - " + a.EnName })
            .ToListAsync();
    }

    public async Task<ResultMessage> CreateManualEntryAsync(ErpEntryViewModel model)
    {
        try
        {
            var lines = (model.Lines ?? new List<ErpEntryLineViewModel>())
                .Where(a => a.AccountId > 0 && (a.Debit.GetValueOrDefault() > 0 || a.Credit.GetValueOrDefault() > 0))
                .ToList();
            if (lines.Count < 2)
            {
                return Fail("JournalNeedsTwoLines");
            }

            if (lines.Any(a => (a.Debit.GetValueOrDefault() > 0) == (a.Credit.GetValueOrDefault() > 0) || a.Debit < 0 || a.Credit < 0))
            {
                return Fail("JournalLineInvalid");
            }

            if (Math.Round(lines.Sum(a => a.Debit.GetValueOrDefault()), 2) != Math.Round(lines.Sum(a => a.Credit.GetValueOrDefault()), 2))
            {
                return Fail("JournalNotBalanced");
            }

            var ids = lines.Select(a => a.AccountId).Distinct().ToList();
            if (await _context.Accounts.CountAsync(a => ids.Contains(a.Id) && a.AccountType != AccountType.Collective) != ids.Count)
            {
                return Fail("RecordNotFound");
            }

            var type = model.EntryType is EntryType.CashReceipt or EntryType.CashPayment ? model.EntryType.Value : EntryType.Journal;
            var (arName, enName) = type switch
            {
                EntryType.CashReceipt => ("قبض", "Cash receipt"),
                EntryType.CashPayment => ("صرف", "Cash payment"),
                _ => ("قيد يومي", "Journal")
            };

            var setting = await ErpBillSupport.GetEntrySettingAsync(_context, type, arName, enName);
            var number = await new JournalEntryRepository(_context).GetNextEntryNumberAsync();
            var total = Math.Round(lines.Sum(a => a.Debit.GetValueOrDefault()), 2);

            await _context.EntryMasters.AddAsync(new EntryMaster
            {
                Code = number,
                EntryDate = model.EntryDate == default ? DateTime.Now : model.EntryDate,
                TotalBalance = total,
                CurrencyRate = 1,
                IsPosted = true,
                EntryRemarks = model.Remarks,
                CollectEntryCode = "Manual:" + number,
                EntrySetting = setting,
                CreatedAt = DateTime.Now,
                EntryDetails = lines.Select((line, index) => new EntryDetail
                {
                    EntryRowCode = (short)(index + 1),
                    AccountId = line.AccountId,
                    Debit = Math.Round(line.Debit.GetValueOrDefault(), 2),
                    Credit = Math.Round(line.Credit.GetValueOrDefault(), 2),
                    EntryDetailsRemarks = line.Remarks,
                    CreatedAt = DateTime.Now
                }).ToList()
            });
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true, Data = number };
        }
        catch (Exception ex)
        {
            return Fail(ErrorLog.Message(ex));
        }
    }

    public async Task<ResultMessage> DeleteManualEntryAsync(int id)
    {
        try
        {
            var entry = await _context.EntryMasters
                .Include(a => a.EntryDetails)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entry is null)
            {
                return new ResultMessage { Success = true };
            }

            if (entry.CollectEntryCode is null || !entry.CollectEntryCode.StartsWith("Manual:"))
            {
                return Fail("EntryBelongsToDocument");
            }

            _context.EntryDetails.RemoveRange(entry.EntryDetails);
            _context.EntryMasters.Remove(entry);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
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
