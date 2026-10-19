using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.DAL.Repositories.Definitions;

// connects the restaurant orders with the bill tables: bill setting, item rows and the entry of the order
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

    public async Task<int> GetItemIdAsync(MenuItem dish)
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

        return item.Id;
    }

    public async Task<bool> PostEntryAsync(Order order)
    {
        await RemoveEntryAsync(order.Id);

        var total = Math.Round(order.Bill.TotalBillAmountAfterDiscountAndExtra, 2);
        var paid = order.OnAccount ? 0 : total;

        int? customerAccountId = null;
        if (order.OnAccount && order.CustomerId.HasValue)
        {
            customerAccountId = await _context.Customers.AsNoTracking()
                .Where(a => a.Id == order.CustomerId.Value)
                .Select(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        var entrySetting = await _billSupport.GetEntrySettingAsync(EntryType.JournalSales, "مبيعات يومية", "Daily sales");
        return await new JournalEntryRepository(_context).PostBillEntryAsync(order.Bill, InvoiceType.Sales, paid, total - paid, customerAccountId, entrySetting);
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
                    detail.Account.Adjust(detail.Account.BalanceType == AccountBalanceType.Debit ? detail.Credit - detail.Debit : detail.Debit - detail.Credit);
                }
            }

            _context.EntryDetails.RemoveRange(entry.EntryDetails);
        }

        _context.EntryMasters.RemoveRange(entries);
    }
}
