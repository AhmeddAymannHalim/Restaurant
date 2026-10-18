using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Repositories.Definitions;

public class OrderLedgerRepository : IOrderLedgerRepository
{
    private const string ItemCodePrefix = "RST-M";

    private readonly ApplicationDbContext _context;
    private readonly IBillSupportRepository _billSupport;

    public OrderLedgerRepository(ApplicationDbContext context)
    {
        _context = context;
        _billSupport = new BillSupportRepository(context);
    }

    public async Task<BillSetting> GetSettingAsync()
    {
        return await _billSupport.GetBillSettingAsync(BillType.Sales, "مبيعات", "Sales");
    }

    public async Task<Dictionary<int, int>> GetItemIdsAsync(IEnumerable<MenuItem> dishes)
    {
        var ids = new Dictionary<int, int>();

        foreach (var dish in dishes)
        {
            var code = ItemCodePrefix + dish.Id;
            var item = await _context.Items.FirstOrDefaultAsync(a => a.Code == code);
            if (item == null)
            {
                item = new Item { Code = code, CreatedAt = DateTime.Now };
                await _context.Items.AddAsync(item);
            }

            item.ArName = dish.ArName;
            item.EnName = dish.EnName;
            await _context.SaveChangesAsync();
            ids[dish.Id] = item.Id;
        }

        return ids;
    }

    public async Task PostEntryAsync(Order order)
    {
        await RemoveEntryAsync(order.Id);
        await new JournalEntryRepository(_context).EnsureDefaultAccountsAsync();

        var bill = order.Bill;
        var total = Math.Round(bill.TotalBillAmountAfterDiscountAndExtra, 2);
        if (total <= 0)
        {
            return;
        }

        var entrySetting = await _billSupport.GetEntrySettingAsync(EntryType.JournalSales, "مبيعات يومية", "Daily sales");

        var cashAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodeRepository.Cash);
        var receivableAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodeRepository.AccountsReceivable);
        var salesAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == AccountCodeRepository.SalesRevenue);
        if (salesAccount == null)
        {
            return;
        }

        Account customerAccount = null;
        if (order.OnAccount && order.CustomerId.HasValue)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == order.CustomerId.Value);
            if (customer != null && customer.AccountId.HasValue)
            {
                customerAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == customer.AccountId.Value);
            }
        }

        Account debitAccount;
        if (customerAccount != null)
        {
            debitAccount = customerAccount;
        }
        else if (order.OnAccount)
        {
            debitAccount = receivableAccount;
        }
        else
        {
            debitAccount = cashAccount;
        }

        if (debitAccount == null)
        {
            return;
        }

        int? boxOrBankId = null;
        if (!order.OnAccount)
        {
            boxOrBankId = debitAccount.Id;
        }

        var debitLine = new EntryDetail
        {
            EntryRowCode = 1,
            AccountId = debitAccount.Id,
            Debit = total,
            Credit = 0,
            EntryDetailsRemarks = order.OrderCode,
            CreatedAt = DateTime.Now
        };

        var creditLine = new EntryDetail
        {
            EntryRowCode = 2,
            AccountId = salesAccount.Id,
            Debit = 0,
            Credit = total,
            EntryDetailsRemarks = order.OrderCode,
            CreatedAt = DateTime.Now
        };

        var entry = new EntryMaster
        {
            Code = order.OrderCode,
            EntryDate = bill.Date,
            TotalBalance = total,
            CurrencyRate = 1,
            IsPosted = true,
            EntryRemarks = "Order " + order.OrderCode,
            AccountBoxOrBankId = boxOrBankId,
            BillId = bill.Id,
            EntrySetting = entrySetting,
            CreatedAt = DateTime.Now,
            EntryDetails = new List<EntryDetail> { debitLine, creditLine }
        };
        await _context.EntryMasters.AddAsync(entry);

        if (customerAccount == null)
        {
            debitAccount.Adjust(total);
        }

        salesAccount.Adjust(-total);
    }

    public async Task RemoveEntryAsync(int billId)
    {
        var entries = await _context.EntryMasters
            .Include(a => a.EntryDetails).ThenInclude(a => a.Account)
            .Where(a => a.BillId == billId)
            .ToListAsync();

        foreach (var entry in entries)
        {
            foreach (var detail in entry.EntryDetails)
            {
                if (detail.Account == null)
                {
                    continue;
                }

                var isCustomerOrSupplier = detail.Account.AccountType == AccountType.Customer
                    || detail.Account.AccountType == AccountType.Supplier;
                if (!isCustomerOrSupplier)
                {
                    detail.Account.Adjust(-(detail.Debit - detail.Credit));
                }
            }

            _context.EntryDetails.RemoveRange(entry.EntryDetails);
        }

        _context.EntryMasters.RemoveRange(entries);
    }
}
