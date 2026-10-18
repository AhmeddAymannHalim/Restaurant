using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class StockRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task AddStockAsync(ApplicationDbContext context, int storeId, int productId, decimal quantity)
    {
        var item = new Item { Code = BillSupportRepository.ItemCodePrefix + productId, ArName = "p", EnName = "p" };
        var unit = await context.Units.FirstOrDefaultAsync(a => a.Code == StockRepository.BaseUnitCode);
        if (unit == null)
        {
            unit = new Unit { Code = StockRepository.BaseUnitCode, ArName = "u", EnName = "u" };
        }

        context.StoreItemUnits.Add(new StoreItemUnit { StoreId = storeId, Item = item, Unit = unit, Quantity = quantity });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task QuantitiesAsync_ListsEveryProductInEveryStore()
    {
        using var context = CreateContext();
        await AddStockAsync(context, 1, 10, 5);
        await AddStockAsync(context, 2, 10, 7);
        await AddStockAsync(context, 1, 11, 9);
        var repository = new StockRepository(context);

        var quantities = await repository.QuantitiesAsync();

        Assert.Equal(3, quantities.Count);
        Assert.Equal(5, repository.FindQuantity(quantities, 1, 10));
        Assert.Equal(7, repository.FindQuantity(quantities, 2, 10));
        Assert.Equal(9, repository.FindQuantity(quantities, 1, 11));
    }

    [Fact]
    public async Task QuantitiesAsync_OneStore_OnlyListsThatStore()
    {
        using var context = CreateContext();
        await AddStockAsync(context, 1, 10, 5);
        await AddStockAsync(context, 2, 10, 7);
        var repository = new StockRepository(context);

        var quantities = await repository.QuantitiesAsync(storeId: 2);

        var only = Assert.Single(quantities);
        Assert.Equal(2, only.StoreId);
        Assert.Equal(10, only.ProductId);
        Assert.Equal(7, only.Quantity);
    }

    [Fact]
    public async Task QuantitiesAsync_RowWithNoQuantity_CountsAsZero()
    {
        using var context = CreateContext();
        await AddStockAsync(context, 1, 10, 0);
        var repository = new StockRepository(context);

        var quantities = await repository.QuantitiesAsync();

        Assert.Equal(0, repository.FindQuantity(quantities, 1, 10));
    }

    [Fact]
    public async Task QuantitiesAsync_ItemsThatAreNotProducts_AreIgnored()
    {
        using var context = CreateContext();
        var unit = new Unit { Code = StockRepository.BaseUnitCode, ArName = "u", EnName = "u" };
        var other = new Item { Code = "SOMETHING-ELSE", ArName = "x", EnName = "x" };
        context.StoreItemUnits.Add(new StoreItemUnit { StoreId = 1, Item = other, Unit = unit, Quantity = 50 });
        await context.SaveChangesAsync();
        var repository = new StockRepository(context);

        var quantities = await repository.QuantitiesAsync();

        Assert.Empty(quantities);
    }

    [Fact]
    public void FindQuantity_ProductNeverStocked_IsZero()
    {
        using var context = CreateContext();
        var repository = new StockRepository(context);

        Assert.Equal(0, repository.FindQuantity(new List<Restaurant.DAL.Helpers.StockQuantity>(), 1, 10));
    }

    [Fact]
    public async Task FindAsync_GivesTheRowOfThatStoreAndProduct()
    {
        using var context = CreateContext();
        await AddStockAsync(context, 1, 10, 5);
        await AddStockAsync(context, 2, 10, 7);
        var repository = new StockRepository(context);

        var row = await repository.FindAsync(2, 10);

        Assert.Equal(7, row.Quantity);
        Assert.Null(await repository.FindAsync(3, 10));
        Assert.Null(await repository.FindAsync(1, 99));
    }
}
