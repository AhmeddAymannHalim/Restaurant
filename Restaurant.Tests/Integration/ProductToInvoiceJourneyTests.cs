using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests;

// One product from the first save to the last delete, every number checked by hand:
// potato is counted in grams (the base unit, rate 1) and sold in kilos (rate 1000).
public class ProductToInvoiceJourneyTests : IDisposable
{
    private readonly TestDb _db = new(invoiceAccounts: true);

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Db => _db.Context;

    private async Task<decimal> Stock(int storeId, int productId)
    {
        Db.ChangeTracker.Clear();
        return (await new StockRepository(Db).FindAsync(storeId, productId))?.Quantity ?? 0;
    }

    private async Task<decimal> Balance(int accountId)
    {
        Db.ChangeTracker.Clear();
        return (await Db.Accounts.AsNoTracking().SingleAsync(a => a.Id == accountId)).BalanceValue;
    }


    [Fact]
    public async Task Potato_FromProductToDishToPurchaseAndSale_EveryNumberIsRight()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "Store" };
        var gram = new UnitPrice { Code = "g", ArName = "Gram", EnName = "Gram" };
        var kilo = new UnitPrice { Code = "k", ArName = "Kilo", EnName = "Kilo" };
        var section = new MenuSection { Code = "1", ArName = "Section", EnName = "Section" };
        var supplier = new Supplier { Account = new Account { Code = "S", ArName = "Sup", EnName = "Sup", AccountType = AccountType.Supplier, BalanceType = AccountBalanceType.Credit } };
        var customer = new Customer { Account = new Account { Code = "C", ArName = "Cus", EnName = "Cus", AccountType = AccountType.Customer, BalanceType = AccountBalanceType.Debit } };
        Db.AddRange(store, gram, kilo, section, supplier, customer);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        // ---- 1. the product: 10000 grams in stock, a gram is priced 0.02 and a kilo 20, so the cost is 0.02 per gram
        var products = new ProductRepository(Db, _db.Http);
        var potato = new Product
        {
            ArName = "Potato", EnName = "Potato", Cost = 0.02m,
            Units = new List<ProductUnit>
            {
                new() { UnitPriceId = gram.Id, TransRate = 1, Price = 0.02m },
                new() { UnitPriceId = kilo.Id, TransRate = 1000, Price = 20 }
            }
        };
        var saved = await products.UpsertProductAsync(potato);
        Assert.True(saved.Success, saved.Message);
        Db.ChangeTracker.Clear();
        var productId = (await Db.Products.AsNoTracking().SingleAsync()).Id;
        await new StockRepository(Db).MoveAsync(store.Id, productId, 10000, StockMovementSource.OpeningBalance, "1");
        await Db.SaveChangesAsync();
        Assert.Equal(0.02m, (await Db.Products.AsNoTracking().SingleAsync()).Cost);

        Assert.Equal(10000, await Stock(store.Id, productId));
        var opened = await products.GetProductByIdAsync(productId);
        Assert.Equal(1, opened.Units.Single(a => a.UnitPriceId == gram.Id).TransRate);
        Assert.Equal(1000, opened.Units.Single(a => a.UnitPriceId == kilo.Id).TransRate);
        Assert.Equal(20, opened.Units.Single(a => a.UnitPriceId == kilo.Id).Price);
        Assert.Equal(0.02m, opened.Units.Single(a => a.UnitPriceId == gram.Id).Price);
        Db.ChangeTracker.Clear();

        // ---- 2. the dish: 0.2 kilo + 100 grams of potato = 300 grams, the rates come from the product
        var menu = new MenuItemRepository(Db, _db.Http);
        var dish = new MenuItem
        {
            ArName = "Fries", EnName = "Fries", Price = 20, StoreId = store.Id, MenuSectionId = section.Id,
            Components = new List<MenuItemComponent>
            {
                new() { ProductId = productId, UnitPriceId = kilo.Id, Quantity = 0.2m, TransRate = 7 },
                new() { ProductId = productId, UnitPriceId = gram.Id, Quantity = 100, TransRate = 9 }
            }
        };
        var dishSaved = await menu.UpsertMenuItemAsync(dish, null);
        Assert.True(dishSaved.Success, dishSaved.Message);
        Db.ChangeTracker.Clear();
        var components = await Db.MenuItemComponents.AsNoTracking().OrderBy(a => a.UnitPriceId).ToListAsync();
        Assert.Equal(1000, components.Single(a => a.UnitPriceId == kilo.Id).TransRate);
        Assert.Equal(1, components.Single(a => a.UnitPriceId == gram.Id).TransRate);
        Assert.Equal(300, components.Sum(a => a.Quantity * a.TransRate));

        // ---- 3. a purchase: 5 kilos at 20 + 500 grams at 0.03 = 100 + 15 = 115, paid 100
        var invoices = new InvoiceRepository(Db, new JournalEntryRepository(Db));
        InvoiceViewModel Purchase(int id, decimal kilos) => new()
        {
            Id = id, InvoiceType = InvoiceType.Purchases, StoreId = store.Id, SupplierId = supplier.Id, Paid = 100, PayType = Shared.Enums.PayType.Later, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = productId, UnitPriceId = kilo.Id, Quantity = kilos, Price = 20 },
                new() { ProductId = productId, UnitPriceId = gram.Id, Quantity = 500, Price = 0.03m }
            }
        };

        var purchase = await invoices.UpsertInvoiceAsync(Purchase(0, 5));
        Assert.True(purchase.Success, purchase.Message);
        Db.ChangeTracker.Clear();
        var purchaseBill = await Db.BillMasters.AsNoTracking().Include(a => a.BillDetails).SingleAsync();
        Assert.Equal(115, purchaseBill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(new[] { 100m, 15m }, purchaseBill.BillDetails.OrderBy(a => a.RowNumber).Select(a => a.TotalAmount ?? 0).ToArray());
        Assert.Equal(new[] { 1000m, 1m }, purchaseBill.BillDetails.OrderBy(a => a.RowNumber).Select(a => a.TransRate ?? 0).ToArray());
        Assert.Equal(10000 + 5000 + 500, await Stock(store.Id, productId));
        // the cost per gram is the weighted average: (10000 x 0.02 + 115) / 15500
        Assert.Equal(315m / 15500m, (await Db.Products.AsNoTracking().SingleAsync()).Cost, 6);
        // each unit remembers the price it was last bought at
        var kiloRow = await Db.ItemUnits.AsNoTracking().SingleAsync(a => a.Unit.Code == BillSupportRepository.UnitCodePrefix + kilo.Id);
        var gramRow = await Db.ItemUnits.AsNoTracking().SingleAsync(a => a.Unit.Code == BillSupportRepository.UnitCodePrefix + gram.Id);
        Assert.Equal(20, kiloRow.LastPurchase);
        Assert.Equal(0.03m, gramRow.LastPurchase);
        // the ERP keeps the journal entry id on the bill, and the entry points back with BillId
        var purchaseEntry = await Db.EntryMasters.AsNoTracking().SingleAsync(a => a.BillId == purchaseBill.Id);
        Assert.Equal(purchaseEntry.Id, purchaseBill.EntryId);
        // the down payment is the bill's own payment line (ERP PaymentMethod), on the pay account of the bill setting
        var purchaseLines = await Db.PaymentMethods.AsNoTracking().Where(a => a.BillMasterId == purchaseBill.Id).ToListAsync();
        Assert.Equal(new[] { 100m }, purchaseLines.Select(a => a.AmountPaided).ToArray());
        Assert.NotNull(purchaseLines.Single().AccountPayId);
        // 115 bought, 100 paid: 15 is still owed to the supplier
        Assert.Equal(15, await Balance(supplier.AccountId.Value));

        // ---- 4. a sale: 2 kilos at 30 + 250 grams at 0.03 = 60 + 7.5 = 67.5, all paid
        var sale = await invoices.UpsertInvoiceAsync(new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Sales, StoreId = store.Id, CustomerId = customer.Id, Paid = 67.5m, PayType = Shared.Enums.PayType.Later, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = productId, UnitPriceId = kilo.Id, Quantity = 2, Price = 30 },
                new() { ProductId = productId, UnitPriceId = gram.Id, Quantity = 250, Price = 0.03m }
            }
        });
        Assert.True(sale.Success, sale.Message);
        Db.ChangeTracker.Clear();
        var saleBill = await Db.BillMasters.AsNoTracking().OrderByDescending(a => a.Id).FirstAsync();
        Assert.Equal(67.5m, saleBill.TotalBillAmountAfterDiscountAndExtra);
        Assert.Equal(15500 - 2250, await Stock(store.Id, productId));
        // cost is the weighted average per gram: 2250 x 315 / 15500
        Assert.Equal(2250m * 315m / 15500m, saleBill.BillCost.Value, 4);
        Assert.Equal(67.5m - 2250m * 315m / 15500m, saleBill.BillProfit.Value, 4);
        Assert.Equal(0, await Balance(customer.AccountId.Value));

        // ---- 5. the sale is edited to 1 kilo: the 2250 grams come back, 1250 grams go out
        Db.ChangeTracker.Clear();
        var edited = await invoices.UpsertInvoiceAsync(new InvoiceViewModel
        {
            Id = saleBill.Id, InvoiceType = InvoiceType.Sales, StoreId = store.Id, CustomerId = customer.Id, Paid = 30, PayType = Shared.Enums.PayType.Later, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = productId, UnitPriceId = kilo.Id, Quantity = 1, Price = 30 },
                new() { ProductId = productId, UnitPriceId = gram.Id, Quantity = 250, Price = 0.03m }
            }
        });
        Assert.True(edited.Success, edited.Message);
        Assert.Equal(15500 - 1250, await Stock(store.Id, productId));
        // 37.5 sold, 30 paid: the customer owes 7.5
        Assert.Equal(7.5m, await Balance(customer.AccountId.Value));
        Db.ChangeTracker.Clear();
        Assert.Equal(2, await Db.BillDetails.CountAsync(a => a.BillMasterId == saleBill.Id));
        // the entry was rebuilt by the edit: the bill points at the new one
        var editedEntry = await Db.EntryMasters.AsNoTracking().SingleAsync(a => a.BillId == saleBill.Id);
        Assert.Equal(editedEntry.Id, (await Db.BillMasters.AsNoTracking().SingleAsync(a => a.Id == saleBill.Id)).EntryId);
        // the payment line was replaced by the new paid amount, not added to the old one
        Assert.Equal(new[] { 30m }, await Db.PaymentMethods.AsNoTracking().Where(a => a.BillMasterId == saleBill.Id).Select(a => a.AmountPaided).ToArrayAsync());
        Assert.Equal(30m, (await invoices.GetInvoiceByIdAsync(saleBill.Id)).Paid);

        // ---- 6. a sale of more than what is in stock is refused and changes nothing
        Db.ChangeTracker.Clear();
        var tooMuch = await invoices.UpsertInvoiceAsync(new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Sales, StoreId = store.Id, CustomerId = customer.Id, AutoPost = true, AutoStock = true,
            Items = new List<InvoiceItemViewModel> { new() { ProductId = productId, UnitPriceId = kilo.Id, Quantity = 100, Price = 30 } }
        });
        Assert.Equal("InsufficientStock", tooMuch.Message);
        Assert.Equal(15500 - 1250, await Stock(store.Id, productId));

        // ---- 7. the kilo cannot be taken off the product while the dish uses it
        Db.ChangeTracker.Clear();
        var withoutKilo = new Product
        {
            Id = productId, ArName = "Potato", EnName = "Potato", Cost = 0.03m,
            Units = new List<ProductUnit> { new() { UnitPriceId = gram.Id, TransRate = 1, IsDefault = true } }
        };
        var removed = await products.UpsertProductAsync(withoutKilo);
        Assert.Equal("CannotDeleteLinkedRecord", removed.Message);
        Db.ChangeTracker.Clear();
        Assert.Equal(2, await Db.ItemUnits.CountAsync(a => a.Item.Code == "RST-P" + productId && a.Unit.Code.StartsWith("RST-U")));

        // ---- 8. both invoices are deleted: stock, balances and entries are back to the start
        Db.ChangeTracker.Clear();
        Assert.True((await invoices.DeleteInvoiceAsync(saleBill.Id)).Success);
        Assert.Equal(15500, await Stock(store.Id, productId));
        Assert.Equal(0, await Balance(customer.AccountId.Value));
        Db.ChangeTracker.Clear();
        Assert.True((await invoices.DeleteInvoiceAsync(purchaseBill.Id)).Success);
        Assert.Equal(10000, await Stock(store.Id, productId));
        Assert.Equal(0, await Balance(supplier.AccountId.Value));
        Db.ChangeTracker.Clear();
        Assert.Empty(await Db.BillMasters.AsNoTracking().ToListAsync());
        Assert.Empty(await Db.EntryMasters.AsNoTracking().ToListAsync());
        Assert.Empty(await Db.PaymentMethods.AsNoTracking().ToListAsync());

        // every movement adds up to what the store holds
        Assert.Equal(10000, await Db.StoreProductMovements.Where(a => a.ProductId == productId).SumAsync(a => a.QuantityDelta));
    }
}
