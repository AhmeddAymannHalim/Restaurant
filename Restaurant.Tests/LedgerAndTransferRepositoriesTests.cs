using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// Manual journal entries, store-to-store transfers and the bill list / posting.
public class LedgerAndTransferRepositoriesTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Account> SeedAccount(string code, AccountType type = AccountType.Normal, AccountBalanceType balance = AccountBalanceType.Debit)
    {
        var account = new Account { Code = code, ArName = "Acc" + code, EnName = "Acc" + code, AccountType = type, BalanceType = balance };
        _db.Context.Accounts.Add(account);
        await _db.Context.SaveChangesAsync();
        return account;
    }

    private async Task<decimal> Balance(Account account) =>
        (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id)).BalanceValue;

    private static EntryViewModel Entry(params (int account, decimal debit, decimal credit)[] lines) => new()
    {
        EntryDate = new DateTime(2030, 1, 1),
        Remarks = "manual",
        Lines = lines.Select(a => new EntryLineViewModel { AccountId = a.account, Debit = a.debit, Credit = a.credit }).ToList()
    };

    // ---------------------------------------------------------------- manual entries

    [Fact]
    public async Task ManualEntry_Balanced_IsSavedAndMovesEveryAccountBalance()
    {
        var sut = new EntryRepository(_db.Context);
        var cash = await SeedAccount("1");
        var capital = await SeedAccount("2", balance: AccountBalanceType.Credit);

        var result = await sut.CreateManualEntryAsync(Entry((cash.Id, 500, 0), (capital.Id, 0, 500)));

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Assert.Equal(500, await Balance(cash));
        Assert.Equal(500, await Balance(capital));
        var saved = await sut.GetEntryByIdAsync((await _db.Context.EntryMasters.AsNoTracking().SingleAsync()).Id);
        Assert.True(saved.IsManual);
        Assert.Equal(2, saved.Lines.Count);
        Assert.Equal(500, saved.Total);
    }

    [Fact]
    public async Task ManualEntry_OnCustomerAccount_MovesTheCustomerBalanceToo()
    {
        var sut = new EntryRepository(_db.Context);
        var customer = await SeedAccount("C", AccountType.Customer);
        var sales = await SeedAccount("S", balance: AccountBalanceType.Credit);

        await sut.CreateManualEntryAsync(Entry((customer.Id, 120, 0), (sales.Id, 0, 120)));

        Assert.Equal(120, await Balance(customer));
    }

    [Theory]
    [InlineData(100, 0, 0, 90, "JournalNotBalanced")]
    [InlineData(100, 100, 0, 100, "JournalLineInvalid")]
    [InlineData(-100, 0, 0, -100, "JournalLineInvalid")]
    public async Task ManualEntry_Invalid_IsRejectedAndChangesNothing(double debit1, double credit1, double debit2, double credit2, string expected)
    {
        var sut = new EntryRepository(_db.Context);
        var a = await SeedAccount("1");
        var b = await SeedAccount("2", balance: AccountBalanceType.Credit);

        var result = await sut.CreateManualEntryAsync(Entry((a.Id, (decimal)debit1, (decimal)credit1), (b.Id, (decimal)debit2, (decimal)credit2)));

        Assert.False(result.Success);
        Assert.Equal(expected, result.Message);
        Assert.Empty(_db.Context.EntryMasters);
        Assert.Equal(0, await Balance(a));
    }

    [Fact]
    public async Task ManualEntry_NegativeLineAmongGoodLines_IsNotSilentlyDropped()
    {
        var sut = new EntryRepository(_db.Context);
        var a = await SeedAccount("1");
        var b = await SeedAccount("2", balance: AccountBalanceType.Credit);
        var c = await SeedAccount("3");

        var result = await sut.CreateManualEntryAsync(Entry((a.Id, 100, 0), (b.Id, 0, 100), (c.Id, -5, 0)));

        Assert.False(result.Success);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task ManualEntry_OneLine_NeedsTwoLines()
    {
        var sut = new EntryRepository(_db.Context);
        var a = await SeedAccount("1");

        Assert.Equal("JournalNeedsTwoLines", (await sut.CreateManualEntryAsync(Entry((a.Id, 10, 0)))).Message);
    }

    [Fact]
    public async Task ManualEntry_UnknownOrCollectiveAccount_ReturnsRecordNotFound()
    {
        var sut = new EntryRepository(_db.Context);
        var a = await SeedAccount("1");
        var group = await SeedAccount("G", AccountType.Collective);

        Assert.Equal("RecordNotFound", (await sut.CreateManualEntryAsync(Entry((a.Id, 10, 0), (999, 0, 10)))).Message);
        Assert.Equal("RecordNotFound", (await sut.CreateManualEntryAsync(Entry((a.Id, 10, 0), (group.Id, 0, 10)))).Message);
    }

    [Fact]
    public async Task ManualEntry_Delete_GivesTheBalancesBack()
    {
        var sut = new EntryRepository(_db.Context);
        var cash = await SeedAccount("1");
        var capital = await SeedAccount("2", balance: AccountBalanceType.Credit);
        await sut.CreateManualEntryAsync(Entry((cash.Id, 500, 0), (capital.Id, 0, 500)));
        var id = (await _db.Context.EntryMasters.AsNoTracking().SingleAsync()).Id;
        _db.Context.ChangeTracker.Clear();

        var result = await sut.DeleteManualEntryAsync(id);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, await Balance(cash));
        Assert.Equal(0, await Balance(capital));
        Assert.Empty(_db.Context.EntryMasters);
        Assert.Empty(_db.Context.EntryDetails);
    }

    [Fact]
    public async Task ManualEntry_DeleteOfDocumentEntry_IsBlockedAndUnknownIsSuccess()
    {
        var sut = new EntryRepository(_db.Context);
        var setting = new EntrySetting { EntryType = EntryType.CustomerPayment, ArName = "a", EnName = "b" };
        _db.Context.EntryMasters.Add(new EntryMaster { Code = "9", EntryDate = DateTime.Today, CollectEntryCode = "CustomerPaymentVoucher:4", EntrySetting = setting });
        await _db.Context.SaveChangesAsync();
        var id = (await _db.Context.EntryMasters.AsNoTracking().SingleAsync()).Id;

        Assert.Equal("EntryBelongsToDocument", (await sut.DeleteManualEntryAsync(id)).Message);
        Assert.True((await sut.DeleteManualEntryAsync(404)).Success);
    }

    [Fact]
    public async Task ManualEntry_CashReceiptType_IsKeptAndPagingFiltersByType()
    {
        var sut = new EntryRepository(_db.Context);
        var a = await SeedAccount("1");
        var b = await SeedAccount("2", balance: AccountBalanceType.Credit);
        var receipt = Entry((a.Id, 10, 0), (b.Id, 0, 10));
        receipt.EntryType = EntryType.CashReceipt;
        await sut.CreateManualEntryAsync(receipt);
        await sut.CreateManualEntryAsync(Entry((a.Id, 20, 0), (b.Id, 0, 20)));
        var request = new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) };

        var all = await sut.GetPagedEntriesAsync(request);
        request.Type = nameof(EntryType.CashReceipt);
        var onlyReceipts = await sut.GetPagedEntriesAsync(request);

        Assert.Equal(2, all.TotalRecords);
        Assert.Single(onlyReceipts.Data);
        Assert.Equal("2", (await sut.GetPostingAccountOptionsAsync()).Count().ToString());
    }

    // ---------------------------------------------------------------- store transfers

    private async Task<(Store from, Store to, Product product)> SeedStores(decimal fromQuantity)
    {
        var from = new Store { Code = "1", ArName = "From", EnName = "From" };
        var to = new Store { Code = "2", ArName = "To", EnName = "To" };
        var product = new Product { Code = "1", ArName = "P", EnName = "P" };
        _db.Context.AddRange(from, to, product);
        await _db.Context.SaveChangesAsync();
        await new StockRepository(_db.Context).AdjustAsync(from.Id, product.Id, fromQuantity);
        _db.Context.ChangeTracker.Clear();
        return (from, to, product);
    }

    private async Task<decimal> StockOf(Store store, Product product)
    {
        _db.Context.ChangeTracker.Clear();
        return (await new StockRepository(_db.Context).FindAsync(store.Id, product.Id))?.Quantity ?? 0;
    }

    private static StoreConvert Convert(Store from, Store to, params (int product, decimal quantity)[] lines) => new()
    {
        FromStoreId = from.Id,
        ToStoreId = to.Id,
        StoreConvertProducts = lines.Select(a => new StoreConvertProduct { ProductId = a.product, Quantity = a.quantity }).ToList()
    };

    [Fact]
    public async Task Convert_MovesStockAndSumsDuplicateProducts()
    {
        var sut = new StoreConvertRepository(_db.Context);
        var (from, to, product) = await SeedStores(10);

        var result = await sut.CreateConvertAsync(Convert(from, to, (product.Id, 3), (product.Id, 2)));

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Assert.Equal(5, await StockOf(from, product));
        Assert.Equal(5, await StockOf(to, product));
        Assert.Equal(2, await _db.Context.StoreProductMovements.CountAsync(a => a.Source == StockMovementSource.StoreConvert));
        Assert.Equal(5, (await sut.GetConvertByIdAsync(1)).StoreConvertProducts.Single().Quantity);
    }

    [Fact]
    public async Task Convert_MoreThanAvailable_IsRejectedAndNothingMoves()
    {
        var sut = new StoreConvertRepository(_db.Context);
        var (from, to, product) = await SeedStores(4);

        var result = await sut.CreateConvertAsync(Convert(from, to, (product.Id, 5)));

        Assert.Equal("InsufficientStock", result.Message);
        Assert.Equal(4, await StockOf(from, product));
        Assert.Equal(0, await StockOf(to, product));
        Assert.Empty(_db.Context.StoreConverts);
    }

    [Fact]
    public async Task Convert_SameStoreEmptyOrNonPositive_IsRejected()
    {
        var sut = new StoreConvertRepository(_db.Context);
        var (from, to, product) = await SeedStores(10);

        Assert.Equal("SameStore", (await sut.CreateConvertAsync(Convert(from, from, (product.Id, 1)))).Message);
        Assert.Equal("AtLeastOneItem", (await sut.CreateConvertAsync(Convert(from, to))).Message);
        Assert.Equal("Val.GreaterThanZero", (await sut.CreateConvertAsync(Convert(from, to, (product.Id, -5)))).Message);
        Assert.Equal("Val.GreaterThanZero", (await sut.CreateConvertAsync(Convert(from, to, (product.Id, 0)))).Message);
        Assert.Equal(10, await StockOf(from, product));
        Assert.Equal(0, await StockOf(to, product));
    }

    [Fact]
    public async Task Convert_Delete_MovesTheStockBack_UnlessItWasAlreadyUsed()
    {
        var sut = new StoreConvertRepository(_db.Context);
        var (from, to, product) = await SeedStores(10);
        await sut.CreateConvertAsync(Convert(from, to, (product.Id, 6)));
        var stock = new StockRepository(_db.Context);
        await stock.AdjustAsync(to.Id, product.Id, -4);
        _db.Context.ChangeTracker.Clear();

        var blocked = await sut.DeleteConvertAsync(1);
        Assert.Equal("InsufficientStock", blocked.Message);
        Assert.Single(_db.Context.StoreConverts);

        _db.Context.ChangeTracker.Clear();
        await stock.AdjustAsync(to.Id, product.Id, 4);
        _db.Context.ChangeTracker.Clear();
        var ok = await sut.DeleteConvertAsync(1);

        Assert.True(ok.Success, ok.Message);
        Assert.Equal(10, await StockOf(from, product));
        Assert.Equal(0, await StockOf(to, product));
        Assert.True((await sut.DeleteConvertAsync(404)).Success);
        Assert.Equal("1", await sut.GetNextCodeAsync());
    }

    // ---------------------------------------------------------------- bills (cashier orders posting)

    private async Task<(OrderRepository orders, BillRepository bills, int orderId, string user)> PaidOrder()
    {
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var store = new Store { Code = "1", ArName = "S", EnName = "S" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        _db.Context.AddRange(user, store, section);
        await _db.Context.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = store.Id, MenuSectionId = section.Id };
        _db.Context.MenuItems.Add(dish);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var orders = new OrderRepository(_db.Context);
        var result = await orders.CreateOrderAsync(new Order
        {
            Type = OrderType.TakeAway,
            Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } }
        }, user.Id);
        Assert.True(result.Success, result.Message);
        _db.Context.ChangeTracker.Clear();
        return (orders, new BillRepository(_db.Context), int.Parse(result.Data), user.Id);
    }

    [Fact]
    public async Task Bill_PostOrder_WithoutAccountsInTheSetting_IsRefusedAndStaysUnposted()
    {
        var (_, bills, orderId, _) = await PaidOrder();

        var result = await bills.PostBillAsync(orderId);

        Assert.Equal("BillSettingAccountsMissing", result.Message);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task Bill_PostOrder_WithAccounts_CreatesOneEntryAndCannotPostTwice()
    {
        var (_, bills, orderId, _) = await PaidOrder();
        var sales = await SeedAccount("SALES", balance: AccountBalanceType.Credit);
        var cash = await SeedAccount("CASH");
        var setting = await _db.Context.BillSettings.SingleAsync();
        setting.AccountItemId = sales.Id;
        setting.AccountPayId = cash.Id;
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();

        var first = await bills.PostBillAsync(orderId);
        var second = await bills.PostBillAsync(orderId);

        Assert.True(first.Success, first.Message);
        Assert.Equal("BillAlreadyPosted", second.Message);
        Assert.Single(_db.Context.EntryMasters);
        Assert.Equal(40 + 0, await Balance(cash)); // no tax or service configured
        Assert.Equal(40, await Balance(sales));
        var model = await bills.GetBillByIdAsync(orderId);
        Assert.True(model.IsCashier);
        Assert.False(model.CanPost);
        Assert.Single(model.Entries);
        Assert.Equal(40, model.Total);
    }

    [Fact]
    public async Task Bill_PostUnknownOrUnpaid_IsRefused_AndPostPendingPostsWhatIsLeft()
    {
        var (orders, bills, orderId, _) = await PaidOrder();
        Assert.Equal("RecordNotFound", (await bills.PostBillAsync(404)).Message);

        var all = await bills.GetPagedBillsAsync(new PageRequest { FromDate = new DateTime(2000, 1, 1), ToDate = new DateTime(2100, 1, 1) });
        var row = Assert.Single(all.Data);
        Assert.Equal("ORD", row.SettingCode);
        Assert.True(row.CanPost);

        var pending = await bills.PostPendingAsync();
        Assert.False(pending.Success); // no accounts yet, reported with how many were posted before
        Assert.Equal("0", pending.Data);
        Assert.Null(await bills.GetBillByIdAsync(404));
    }
}
