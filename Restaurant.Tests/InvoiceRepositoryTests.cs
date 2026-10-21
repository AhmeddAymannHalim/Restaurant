using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

public class InvoiceRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly InvoiceRepository _sut;
    private readonly StockRepository _stock;

    private Store _store;
    private Product _product;
    private UnitPrice _unit;
    private Supplier _supplier;
    private Customer _customer;

    public InvoiceRepositoryTests()
    {
        _sut = new InvoiceRepository(_db.Context, new JournalEntryRepository(_db.Context));
        _stock = new StockRepository(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    private async Task Seed()
    {
        _store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        _product = new Product { Code = "1", ArName = "Product", EnName = "Product", Cost = 5 };
        _unit = new UnitPrice { Code = "1", ArName = "Unit", EnName = "Unit" };
        _supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        _customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        _db.Context.AddRange(_store, _product, _unit, _supplier, _customer);
        await _db.Context.SaveChangesAsync();
    }

    private InvoiceViewModel Invoice(InvoiceType type, decimal quantity, decimal price, decimal paid = 0, bool autoStock = true)
    {
        var model = new InvoiceViewModel
        {
            InvoiceType = type,
            StoreId = _store.Id,
            Paid = paid,
            AutoStock = autoStock,
            AutoPost = true,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = _product.Id, UnitPriceId = _unit.Id, Quantity = quantity, Price = price }
            }
        };
        if (model.IsSupplierInvoice) model.SupplierId = _supplier.Id;
        else model.CustomerId = _customer.Id;
        return model;
    }

    private async Task<decimal> Stock()
    {
        var row = await _stock.FindAsync(_store.Id, _product.Id);
        return row?.Quantity ?? 0;
    }

    private async Task<decimal> Balance(Account account) =>
        (await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == account.Id)).BalanceValue;

    [Fact]
    public async Task Purchase_Cash_AddsStockAndUpdatesCost()
    {
        await Seed();

        var result = await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 70));

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        Assert.Equal(10, await Stock());
        Assert.Equal(7, (await _db.Context.Products.AsNoTracking().SingleAsync()).Cost);
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        Assert.Equal(70, bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(0, bill.CasherRemainingAmount);
        Assert.Equal(0, await Balance(_supplier.Account));
        var movement = await _db.Context.StoreProductMovements.AsNoTracking().SingleAsync();
        Assert.Equal(10, movement.QuantityDelta);
    }

    [Fact]
    public async Task Purchase_OnCredit_IncreasesWhatWeOweTheSupplier()
    {
        await Seed();

        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 20));

        Assert.Equal(50, await Balance(_supplier.Account));
    }

    [Fact]
    public async Task Sale_OnCredit_IncreasesWhatTheCustomerOwesAndReducesStock()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 5, paid: 50));

        var result = await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Sales, 4, 9, paid: 6));

        Assert.True(result.Success, result.Message);
        Assert.Equal(6, await Stock());
        Assert.Equal(30, await Balance(_customer.Account));
        var bill = await _db.Context.BillMasters.AsNoTracking().OrderBy(a => a.Id).LastAsync();
        Assert.Equal(36 - 20, bill.BillProfit); // 36 total - 4 * 5 cost
    }

    [Fact]
    public async Task Sale_MoreThanStock_IsRejectedAndStockAndBalanceAreUntouched()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 3, 5, paid: 15));

        var result = await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Sales, 4, 9));

        Assert.False(result.Success);
        Assert.Equal("InsufficientStock", result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(3, await Stock());
        Assert.Equal(0, await Balance(_customer.Account));
        Assert.Equal(1, await _db.Context.BillMasters.CountAsync());
    }

    [Fact]
    public async Task SalesReturn_ReversesTheProfitOfTheSale()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 5, paid: 50));

        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Sales, 4, 9, paid: 36));
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.SalesReturn, 2, 9, paid: 18));

        var bills = await _db.Context.BillMasters.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        Assert.Equal(16, bills[1].BillProfit);   // 36 - 4 * 5
        Assert.Equal(-8, bills[2].BillProfit);   // 18 returned - 2 * 5 cost taken back
        Assert.Equal(10, bills[2].BillCost);
    }

    [Fact]
    public async Task SalesReturn_PutsStockBack()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 5, 5, paid: 25));

        var result = await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.SalesReturn, 2, 9, paid: 18));

        Assert.True(result.Success, result.Message);
        Assert.Equal(7, await Stock());
    }

    [Fact]
    public async Task Upsert_NoItems_ReturnsInvoiceNeedsItems()
    {
        await Seed();
        var model = Invoice(InvoiceType.Purchases, 0, 5);

        Assert.Equal("InvoiceNeedsItems", (await _sut.UpsertInvoiceAsync(model)).Message);
    }

    [Fact]
    public async Task Upsert_UnknownStoreOrProductOrPartner_ReturnsRecordNotFound()
    {
        await Seed();
        var noStore = Invoice(InvoiceType.Purchases, 1, 5);
        noStore.StoreId = 999;
        var noProduct = Invoice(InvoiceType.Purchases, 1, 5);
        noProduct.Items[0].ProductId = 999;
        var noPartner = Invoice(InvoiceType.Purchases, 1, 5);
        noPartner.SupplierId = 999;

        Assert.Equal("RecordNotFound", (await _sut.UpsertInvoiceAsync(noStore)).Message);
        Assert.Equal("RecordNotFound", (await _sut.UpsertInvoiceAsync(noProduct)).Message);
        Assert.Equal("RecordNotFound", (await _sut.UpsertInvoiceAsync(noPartner)).Message);
        Assert.Empty(_db.Context.BillMasters);
    }

    [Fact]
    public async Task Upsert_PaidMoreThanTotal_ReturnsPaidTooBig()
    {
        await Seed();

        var result = await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 1, 5, paid: 6));

        Assert.Equal("Val.PaidTooBig", result.Message);
    }

    [Fact]
    public async Task Upsert_DiscountReducesTotal()
    {
        await Seed();
        var model = Invoice(InvoiceType.Purchases, 10, 10, paid: 0);
        model.Discount = 20;

        await _sut.UpsertInvoiceAsync(model);

        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        Assert.Equal(80, bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(80, await Balance(_supplier.Account));
    }

    [Fact]
    public async Task Edit_ChangesQuantityAndRecalculatesStockAndBalance()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 20));
        var billId = (await _db.Context.BillMasters.AsNoTracking().SingleAsync()).Id;
        _db.Context.ChangeTracker.Clear();

        var edit = Invoice(InvoiceType.Purchases, 4, 7, paid: 8);
        edit.Id = billId;
        var result = await _sut.UpsertInvoiceAsync(edit);

        Assert.True(result.Success, result.Message);
        Assert.Equal(4, await Stock());
        Assert.Equal(20, await Balance(_supplier.Account)); // 28 total - 8 paid
        Assert.Single(_db.Context.BillMasters);
        Assert.Single(_db.Context.BillDetails);
    }

    [Fact]
    public async Task Edit_UnknownInvoice_ReturnsRecordNotFound()
    {
        await Seed();
        var edit = Invoice(InvoiceType.Purchases, 1, 5);
        edit.Id = 77;

        Assert.Equal("RecordNotFound", (await _sut.UpsertInvoiceAsync(edit)).Message);
    }

    [Fact]
    public async Task Delete_PutsStockAndBalanceBack()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 20));
        var billId = (await _db.Context.BillMasters.AsNoTracking().SingleAsync()).Id;
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.DeleteInvoiceAsync(billId);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, await Stock());
        Assert.Equal(0, await Balance(_supplier.Account));
        Assert.Empty(_db.Context.BillMasters);
        Assert.Empty(_db.Context.BillDetails);
    }

    [Fact]
    public async Task Delete_WithPayments_IsBlocked()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7));
        var bill = await _db.Context.BillMasters.AsNoTracking().SingleAsync();
        _db.Context.PaymentInvoiceSupplier.Add(new PaymentInvoiceSupplier { SupplierId = _supplier.Id, BillMasterId = bill.Id, Paid = 10, ProcessCode = "1", Date = DateTime.Now });
        await _db.Context.SaveChangesAsync();

        var result = await _sut.DeleteInvoiceAsync(bill.Id);

        Assert.Equal("InvoiceHasPayments", result.Message);
        Assert.Equal(10, await Stock());
    }

    [Fact]
    public async Task Delete_PurchaseWhoseStockWasSold_IsBlocked()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 5, paid: 50));
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Sales, 8, 9, paid: 72));
        var purchaseId = (await _db.Context.BillMasters.AsNoTracking().OrderBy(a => a.Id).FirstAsync()).Id;
        _db.Context.ChangeTracker.Clear();

        var result = await _sut.DeleteInvoiceAsync(purchaseId);

        Assert.Equal("InsufficientStock", result.Message);
        _db.Context.ChangeTracker.Clear();
        Assert.Equal(2, await Stock());
        Assert.Equal(2, await _db.Context.BillMasters.CountAsync());
    }

    [Fact]
    public async Task Delete_Unknown_IsSuccess()
    {
        Assert.True((await _sut.DeleteInvoiceAsync(404)).Success);
    }

    [Fact]
    public async Task Post_UnpostedInvoice_AppliesStockOnceThenRefusesSecondPost()
    {
        await Seed();
        var model = Invoice(InvoiceType.Purchases, 10, 5, paid: 20, autoStock: false);
        model.AutoPost = false;
        await _sut.UpsertInvoiceAsync(model);
        var billId = (await _db.Context.BillMasters.AsNoTracking().SingleAsync()).Id;
        Assert.Equal(0, await Stock());
        Assert.Contains(billId, await _sut.GetUnpostedInvoiceIdsAsync());
        _db.Context.ChangeTracker.Clear();

        var first = await _sut.PostInvoiceAsync(billId);
        var second = await _sut.PostInvoiceAsync(billId);

        Assert.True(first.Success, first.Message);
        Assert.Equal(10, await Stock());
        Assert.Equal(30, await Balance(_supplier.Account));
        Assert.Equal("BillAlreadyPosted", second.Message);
        Assert.Empty(await _sut.GetUnpostedInvoiceIdsAsync());
    }

    [Fact]
    public async Task Post_Unknown_ReturnsRecordNotFound()
    {
        Assert.Equal("RecordNotFound", (await _sut.PostInvoiceAsync(404)).Message);
    }

    [Fact]
    public async Task GetById_ReturnsWhatWasSaved()
    {
        await Seed();
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 20));
        var billId = (await _db.Context.BillMasters.AsNoTracking().SingleAsync()).Id;

        var model = await _sut.GetInvoiceByIdAsync(billId);

        Assert.Equal(InvoiceType.Purchases, model.InvoiceType);
        Assert.Equal(_supplier.Id, model.SupplierId);
        Assert.Equal(20, model.Paid);
        var item = Assert.Single(model.Items);
        Assert.Equal(_product.Id, item.ProductId);
        Assert.Equal(_unit.Id, item.UnitPriceId);
        Assert.Equal(10, item.Quantity);
        Assert.Equal(7, item.Price);
        Assert.Null(await _sut.GetInvoiceByIdAsync(404));
    }

    [Fact]
    public async Task Balances_AndNextCode_Work()
    {
        await Seed();
        Assert.Equal("1", await _sut.GetNextCodeAsync(InvoiceType.Purchases));
        await _sut.UpsertInvoiceAsync(Invoice(InvoiceType.Purchases, 10, 7, paid: 20));

        Assert.Equal("2", await _sut.GetNextCodeAsync(InvoiceType.Purchases));
        Assert.Equal("1", await _sut.GetNextCodeAsync(InvoiceType.Sales));
        Assert.Equal(50, await _sut.GetSupplierBalanceAsync(_supplier.Id));
        Assert.Equal(0, await _sut.GetCustomerBalanceAsync(_customer.Id));
        Assert.Equal(0, await _sut.GetSupplierBalanceAsync(999));
    }
}
