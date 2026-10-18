using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests.Repositories;

public class InvoiceRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (Store store, Product product, UnitPrice unitPrice) SeedCatalog(ApplicationDbContext context)
    {
        var store = new Store { ArName = "مخزن", EnName = "Store" };
        var product = new Product { ArName = "منتج", EnName = "Product", Cost = 5 };
        var unitPrice = new UnitPrice { ArName = "وحدة", EnName = "Unit" };
        context.Stores.Add(store);
        context.Products.Add(product);
        context.UnitPrices.Add(unitPrice);
        return (store, product, unitPrice);
    }

    [Fact]
    public async Task UpsertInvoiceAsync_NewSurplusInvoiceWithoutAutoPostOrAutoStock_CreatesBillWithCorrectTotals()
    {
        // Arrange
        using var context = CreateContext();
        var (store, product, unitPrice) = SeedCatalog(context);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new InvoiceRepository(context, journals);

        var model = new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Surplus,
            StoreId = store.Id,
            Discount = 0,
            Paid = 0,
            AutoPost = false,
            AutoStock = false,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = product.Id, UnitPriceId = unitPrice.Id, Quantity = 5, Price = 10 }
            }
        };

        // Act
        var result = await repository.UpsertInvoiceAsync(model);

        // Assert
        Assert.True(result.Success);
        var bill = await context.BillMasters.AsNoTracking().FirstAsync(a => a.Code == result.Data);
        Assert.Equal(50, bill.TotalAmountBeforeDiscountOrExtra);
        Assert.Equal(50, bill.TotalBillAmountAfterDiscountAndExtra);
        Assert.False(bill.IsEntryPosted);
        Assert.False(bill.IsStorageEffected);
    }

    [Fact]
    public async Task UpsertInvoiceAsync_NoItems_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var (store, _, _) = SeedCatalog(context);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new InvoiceRepository(context, journals);

        var model = new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Surplus,
            StoreId = store.Id,
            AutoPost = false,
            AutoStock = false,
            Items = new List<InvoiceItemViewModel>()
        };

        // Act
        var result = await repository.UpsertInvoiceAsync(model);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("InvoiceNeedsItems", result.Message);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_ExistingInvoice_ReturnsViewModelWithItems()
    {
        // Arrange
        using var context = CreateContext();
        var (store, product, unitPrice) = SeedCatalog(context);
        await context.SaveChangesAsync();

        var journals = new JournalEntryRepository(context);
        var repository = new InvoiceRepository(context, journals);

        var model = new InvoiceViewModel
        {
            InvoiceType = InvoiceType.Surplus,
            StoreId = store.Id,
            Discount = 0,
            Paid = 0,
            AutoPost = false,
            AutoStock = false,
            Items = new List<InvoiceItemViewModel>
            {
                new() { ProductId = product.Id, UnitPriceId = unitPrice.Id, Quantity = 3, Price = 20 }
            }
        };
        var createResult = await repository.UpsertInvoiceAsync(model);
        Assert.True(createResult.Success);
        var createdBill = await context.BillMasters.AsNoTracking().FirstAsync(a => a.Code == createResult.Data);

        // Act
        var invoice = await repository.GetInvoiceByIdAsync(createdBill.Id);

        // Assert
        Assert.NotNull(invoice);
        Assert.Equal(createdBill.Id, invoice.Id);
        Assert.Equal(InvoiceType.Surplus, invoice.InvoiceType);
        Assert.Single(invoice.Items);
        Assert.Equal(3, invoice.Items[0].Quantity);
        Assert.Equal(20, invoice.Items[0].Price);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_BillDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var journals = new JournalEntryRepository(context);
        var repository = new InvoiceRepository(context, journals);

        // Act
        var invoice = await repository.GetInvoiceByIdAsync(999);

        // Assert
        Assert.Null(invoice);
    }
}
