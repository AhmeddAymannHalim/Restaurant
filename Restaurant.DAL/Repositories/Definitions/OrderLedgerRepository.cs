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
        await new JournalEntryRepository(_context).RemoveBillEntriesAsync(billId);
    }

    public void RecalculateBill(Order order)
    {
        var subTotal = order.Lines.Sum(a => a.Price);
        order.Tax = Math.Round(subTotal * order.Bill.BillExtraPercentage / 100, 2);
        var extras = order.Tax + order.Service + order.DeliveryFee;
        var discount = Math.Min(Math.Max(0, order.Bill.BillDiscountValue), subTotal + extras);
        var total = subTotal + extras - discount;
        var cost = order.Lines.Sum(a => a.UnitCost * a.Quantity);

        order.Bill.TotalAmountBeforeDiscountOrExtra = subTotal;
        order.Bill.TotalItemsAmountAfterDiscountAndExtra = subTotal;
        order.Bill.BillDiscountValue = discount;
        order.Bill.TotalDiscountValueOnBillAndItems = discount;
        order.Bill.BillExtraValue = extras;
        order.Bill.TotalExtraValueOnBillAndItems = extras;
        order.Bill.TotalBillAmountAfterDiscountAndExtra = total;
        order.Bill.BillCost = cost;
        order.Bill.BillProfit = total - order.Tax - order.DeliveryFee - cost;
        order.Bill.PayType = order.OnAccount ? PayType.Later : PayType.Cash;
        order.Bill.IsEntryPosted = false;
        order.Bill.AmountPaided = order.Cash && !order.OnAccount ? total : 0;
        order.Bill.CasherRemainingAmount = total - order.Bill.AmountPaided;
        order.Bill.UpdatedAt = DateTime.Now;
    }

    // the payment rows of a paid order: each payment the cashier took (cash, visa, ...) is one row,
    // cash goes to the cash box and the other methods to the card account of the sales setting.
    // returns the message key when the payments are wrong, or null
    public async Task<string> SetPaymentsAsync(Order order, List<Shared.ViewModels.Definitions.OrderPaymentViewModel> payments)
    {
        if (order.Bill.Id != 0)
        {
            _context.PaymentMethods.RemoveRange(await _context.PaymentMethods.Where(a => a.BillMasterId == order.Bill.Id).ToListAsync());
        }

        order.Bill.PaymentMethods = new List<Models.PaymentMethod>();
        if (!order.Cash || order.OnAccount)
        {
            return null;
        }

        var total = Math.Round(order.Bill.TotalBillAmountAfterDiscountAndExtra, 2);
        var lines = payments.Where(a => a.Amount != 0).ToList();
        if (lines.Any(a => a.Amount < 0))
        {
            return "PaymentNegative";
        }

        // one method and no amount typed: the whole bill is paid by it
        if (lines.Count == 0 && total > 0)
        {
            lines.Add(new Shared.ViewModels.Definitions.OrderPaymentViewModel { Method = order.PaymentMethod ?? Shared.Enums.PaymentMethod.Cash, Amount = total });
        }

        var paid = lines.Sum(a => Math.Round(a.Amount, 2));
        if (paid < total - 0.01m)
        {
            return "PaymentsLessThanTotal";
        }

        if (paid > total + 0.01m)
        {
            return "PaymentsMoreThanTotal";
        }

        if (lines.Count == 0)
        {
            return null;
        }

        var setting = await GetSettingAsync();
        order.PaymentMethod = lines[0].Method;
        foreach (var line in lines)
        {
            order.Bill.PaymentMethods.Add(new Restaurant.DAL.Models.PaymentMethod
            {
                RowNumber = (byte)(order.Bill.PaymentMethods.Count + 1),
                AccountPayId = line.Method == Shared.Enums.PaymentMethod.Cash ? setting.AccountPayId : setting.AccountPaymentCardId ?? setting.AccountPayId,
                AmountPaided = Math.Round(line.Amount, 2),
                CreatedAt = DateTime.Now
            });
        }

        // a rounding cent goes on the last row so the rows always add up to the bill
        order.Bill.PaymentMethods.Last().AmountPaided += total - paid;

        return null;
    }

    public async Task RemoveOrderAsync(Order order)
    {
        if (order.Type == OrderType.Hall && order.TableId.HasValue && !order.Cash)
        {
            var freedTable = await _context.Tables.FindAsync(order.TableId.Value);
            if (freedTable is not null)
            {
                freedTable.IsPending = false;
            }
        }

        await RemoveEntryAsync(order.Id);

        var orderLines = order.Lines.Where(a => a.OrderId == order.Id).ToList();
        _context.OrderIngredients.RemoveRange(orderLines.SelectMany(a => a.Ingredients));
        _context.OrderMenuItems.RemoveRange(orderLines);
        _context.BillDetails.RemoveRange(orderLines.Select(a => a.BillDetail).Where(a => a is not null));
        _context.Orders.Remove(order);
        if (order.Bill is not null)
        {
            _context.PaymentMethods.RemoveRange(await _context.PaymentMethods.Where(a => a.BillMasterId == order.Bill.Id).ToListAsync());
            _context.BillMasters.Remove(order.Bill);
        }
    }
}
