using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// Invoices with their journal entries configured (item and pay accounts), edited and deleted: the balances of the accounts must come
// back exactly; and the branches around them: a bill that is not an invoice, a line that is not a product, a save that loses a race.
public class InvoiceBranchTests : IDisposable
{
    private readonly TestHost _host = new();

    public void Dispose() => _host.Dispose();

    private ApplicationDbContext Db => _host.Context;
    private IInvoiceRepository Invoices => _host.Get<IInvoiceRepository>();

    private Store _store;
    private Product _product;
    private UnitPrice _unit;
    private Supplier _supplier;
    private Customer _customer;
    private Account _itemAccount;
    private Account _payAccount;

    private async Task Seed(bool withAccounts)
    {
        _store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        _product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        _unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        _supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        _customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _itemAccount = new Account { Code = "PURCH", ArName = "Purchases", EnName = "Purchases", BalanceType = AccountBalanceType.Debit };
        _payAccount = new Account { Code = "CASH", ArName = "Cash", EnName = "Cash", BalanceType = AccountBalanceType.Debit };
        Db.AddRange(_store, _product, _unit, _supplier, _customer, _itemAccount, _payAccount);
        await Db.SaveChangesAsync();

        if (withAccounts)
        {
            // the bill setting of a purchase is created on first use; its accounts are what the journal entry posts to
            var support = _host.Get<IBillSupportRepository>();
            var info = support.GetInvoiceType(InvoiceType.Purchases);
            var setting = await support.GetBillSettingAsync(info.BillType, info.ArName, info.EnName);
            setting.AccountItemId = _itemAccount.Id;
            setting.AccountPayId = _payAccount.Id;
            await Db.SaveChangesAsync();
        }

        Db.ChangeTracker.Clear();
    }

    private InvoiceViewModel Purchase(decimal quantity, decimal price, decimal paid, int id = 0) => new()
    {
        Id = id, InvoiceType = InvoiceType.Purchases, StoreId = _store.Id, SupplierId = _supplier.Id, Paid = paid, PayType = Shared.Enums.PayType.Later, AutoPost = true, AutoStock = true,
        Items = new List<InvoiceItemViewModel> { new() { ProductId = _product.Id, UnitPriceId = _unit.Id, Quantity = quantity, Price = price } }
    };

    private async Task<decimal> Balance(Account account) =>
        (await Db.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id)).BalanceValue;

    private async Task<decimal> Stock()
    {
        Db.ChangeTracker.Clear();

        return (await new StockRepository(Db).FindAsync(_store.Id, _product.Id))?.Quantity ?? 0;
    }

    // ---------------------------------------------------------------- balances come back exactly

    [Fact]
    public async Task Purchase_PostsItsEntryToTheConfiguredAccounts_EditMovesThemToTheNewAmounts_DeleteGivesThemBack()
    {
        await Seed(withAccounts: true);

        var created = await Invoices.UpsertInvoiceAsync(Purchase(10, 7, 30));
        Assert.True(created.Success, created.Message);
        Db.ChangeTracker.Clear();
        Assert.Equal(70, await Balance(_itemAccount));
        Assert.Equal(-30, await Balance(_payAccount));
        Assert.Equal(10, await Stock());
        var id = (await Db.BillMasters.AsNoTracking().SingleAsync()).Id;

        var edited = await Invoices.UpsertInvoiceAsync(Purchase(5, 7, 10, id));
        Assert.True(edited.Success, edited.Message);
        Db.ChangeTracker.Clear();
        Assert.Equal(35, await Balance(_itemAccount));
        Assert.Equal(-10, await Balance(_payAccount));
        Assert.Equal(5, await Stock());
        Assert.Single(await Db.EntryMasters.AsNoTracking().ToListAsync());

        var deleted = await Invoices.DeleteInvoiceAsync(id);
        Assert.True(deleted.Success, deleted.Message);
        Db.ChangeTracker.Clear();
        Assert.Equal(0, await Balance(_itemAccount));
        Assert.Equal(0, await Balance(_payAccount));
        Assert.Equal(0, await Stock());
        Assert.Empty(await Db.EntryMasters.AsNoTracking().ToListAsync());
    }

    // ---------------------------------------------------------------- a save that loses a race

    [Fact]
    public async Task Invoice_SaveAndDelete_WhenTheRowWasChangedByAnotherUser_AnswerConcurrentUpdate()
    {
        await Seed(withAccounts: true);
        Assert.True((await Invoices.UpsertInvoiceAsync(Purchase(1, 1, 0))).Success);
        Db.ChangeTracker.Clear();
        var id = (await Db.BillMasters.AsNoTracking().SingleAsync()).Id;
        _host.Db.SaveFailure.Failure = new DbUpdateConcurrencyException("forced");

        var saved = await Invoices.UpsertInvoiceAsync(Purchase(2, 1, 0, id));
        var deleted = await Invoices.DeleteInvoiceAsync(id);

        Assert.Equal("ConcurrentUpdate", saved.Message);
        Assert.Equal("ConcurrentUpdate", deleted.Message);
    }

    // ---------------------------------------------------------------- a bill that is not an invoice

    [Fact]
    public async Task Invoice_EditAndDelete_OfACashierOrdersBill_AreRefused()
    {
        await Seed(withAccounts: false);
        var user = new ApplicationUser { UserName = "u", Email = "u@test.com" };
        var section = new MenuSection { Code = "1", ArName = "Sec", EnName = "Sec" };
        Db.AddRange(user, section);
        await Db.SaveChangesAsync();
        var dish = new MenuItem { Code = "1", ArName = "Dish", EnName = "Dish", Price = 40, StoreId = _store.Id, MenuSectionId = section.Id };
        Db.MenuItems.Add(dish);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        var walkIn = new Customer { Account = new Account { Code = "W", ArName = "Walk-in", EnName = "Walk-in", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        Db.Add(walkIn);
        await Db.SaveChangesAsync();
        var created = await new OrderRepository(Db).CreateOrderAsync(new Order { Type = OrderType.TakeAway, CustomerId = walkIn.Id, Lines = new List<OrderMenuItem> { new() { MenuItemId = dish.Id, BillDetail = new BillDetail { Quantity = 1 } } } }, user.Id);
        Assert.True(created.Success, created.Message);
        var orderBillId = int.Parse(created.Data);
        Db.ChangeTracker.Clear();

        var edit = await Invoices.UpsertInvoiceAsync(Purchase(1, 1, 0, orderBillId));
        var delete = await Invoices.DeleteInvoiceAsync(orderBillId);
        var post = await Invoices.PostInvoiceAsync(orderBillId);

        Assert.Equal("BillNotEditable", edit.Message);
        Assert.Equal("BillNotEditable", delete.Message);
        Assert.Equal("RecordNotFound", post.Message);
        Assert.Null(await Invoices.GetInvoiceByIdAsync(orderBillId));
    }

    // ---------------------------------------------------------------- a line that is not a stock product

    [Fact]
    public async Task Invoice_ALineThatIsNotAProduct_IsLeftOutOfTheStockOnEditDeleteAndPosting()
    {
        await Seed(withAccounts: true);
        var postedLater = Purchase(4, 1, 0);
        postedLater.AutoStock = false;
        postedLater.AutoPost = false;
        Assert.True((await Invoices.UpsertInvoiceAsync(postedLater)).Success);
        var applied = Purchase(6, 1, 0);
        Assert.True((await Invoices.UpsertInvoiceAsync(applied)).Success);
        Db.ChangeTracker.Clear();
        var foreign = new Item { Code = "NOT-A-PRODUCT", ArName = "x", EnName = "x" };
        Db.Items.Add(foreign);
        await Db.SaveChangesAsync();
        var bills = await Db.BillMasters.Include(a => a.BillDetails).OrderBy(a => a.Id).ToListAsync();
        foreach (var detail in bills.SelectMany(a => a.BillDetails))
        {
            detail.ItemId = foreign.Id;
        }

        await Db.SaveChangesAsync();
        var pendingId = bills[0].Id;
        var appliedId = bills[1].Id;
        Db.ChangeTracker.Clear();
        var stockBefore = await Stock();

        var posted = await Invoices.PostInvoiceAsync(pendingId);
        Db.ChangeTracker.Clear();
        var edited = await Invoices.UpsertInvoiceAsync(Purchase(1, 1, 0, appliedId));
        Db.ChangeTracker.Clear();
        var deleted = await Invoices.DeleteInvoiceAsync(appliedId);

        Assert.True(posted.Success, posted.Message);
        Assert.True(edited.Success, edited.Message);
        Assert.True(deleted.Success, deleted.Message);
        // the foreign lines neither added nor took back any stock; only the edit's own new line moved 1
        Assert.Equal(stockBefore + 1 - 1, await Stock());
    }

    // ---------------------------------------------------------------- the entry of an invoice

    // the bill and its entry setting are tracked by the context, like the invoice repository has them when it posts
    private async Task<(IJournalEntryRepository journals, BillMaster bill, EntrySetting entry)> BillWithSetting(Action<BillSetting> configure, decimal total = 100)
    {
        var support = _host.Get<IBillSupportRepository>();
        var info = support.GetInvoiceType(InvoiceType.Purchases);
        var setting = await support.GetBillSettingAsync(info.BillType, info.ArName, info.EnName);
        configure(setting);
        await Db.SaveChangesAsync();
        var entry = await support.GetEntrySettingAsync(info.Entry, info.ArName, info.EnName);
        var bill = new BillMaster { Code = "X" + Db.BillMasters.Count(), Date = DateTime.Today, BillSettingId = setting.Id, TotalBillAmountAfterDiscountAndExtra = total };
        Db.Add(bill);
        await Db.SaveChangesAsync();

        return (_host.Get<IJournalEntryRepository>(), bill, entry);
    }

    [Fact]
    public async Task InvoiceEntry_IsNotPosted_WhenTheSettingLacksAnAccountItNeeds()
    {
        await Seed(withAccounts: false);
        var (journals, bill, entry) = await BillWithSetting(s => { s.AccountItemId = null; s.AccountPayId = null; });

        Assert.False(await journals.PostBillEntryAsync(bill, InvoiceType.Purchases, 0, 100, _supplier.AccountId, entry));

        var setting = await Db.BillSettings.SingleAsync();
        setting.AccountItemId = _itemAccount.Id;
        await Db.SaveChangesAsync();
        // paying needs the pay account, an empty bill needs nothing at all
        Assert.False(await journals.PostBillEntryAsync(bill, InvoiceType.Purchases, 50, 50, _supplier.AccountId, entry));
        bill.TotalBillAmountAfterDiscountAndExtra = 0;
        Assert.False(await journals.PostBillEntryAsync(bill, InvoiceType.Purchases, 0, 0, _supplier.AccountId, entry));
    }

    [Fact]
    public async Task InvoiceEntry_NeedsAPartnerAccountForWhatIsOwed_AndOnlyPostsPurchasesSalesAndTheirReturns()
    {
        await Seed(withAccounts: false);
        var (journals, bill, entry) = await BillWithSetting(s => { s.AccountItemId = _itemAccount.Id; s.AccountPayId = _payAccount.Id; });

        Assert.False(await journals.PostBillEntryAsync(bill, InvoiceType.Purchases, 40, 60, null, entry));
        Assert.False(await journals.PostBillEntryAsync(bill, InvoiceType.Shortage, 100, 0, null, entry));
        Assert.True(await journals.PostBillEntryAsync(bill, InvoiceType.Purchases, 40, 60, _supplier.AccountId, entry));
        await Db.SaveChangesAsync();

        Db.ChangeTracker.Clear();
        var lines = await Db.EntryDetails.AsNoTracking().OrderBy(a => a.EntryRowCode).ToListAsync();
        Assert.Equal(new decimal[] { 100, 0, 0 }, lines.Select(a => a.Debit).ToArray());
        Assert.Equal(new decimal[] { 0, 40, 60 }, lines.Select(a => a.Credit).ToArray());
        Assert.Equal(100, (await Db.EntryMasters.AsNoTracking().SingleAsync()).TotalBalance);
    }

    [Fact]
    public async Task JournalSource_OfAKindWithoutAnEntryType_PostsNothingAndSucceeds()
    {
        await Seed(withAccounts: false);
        var journals = _host.Get<IJournalEntryRepository>();
        var lines = new[] { new JournalLineRequest(_itemAccount.Id, 5, 0), new JournalLineRequest(_payAccount.Id, 0, 5) };

        var result = await journals.PostSourceEntryAsync((JournalSourceType)999, 1, DateTime.Today, "x", lines);

        Assert.True(result.Success, result.Message);
        Assert.Empty(await Db.EntryMasters.AsNoTracking().ToListAsync());
    }
}
