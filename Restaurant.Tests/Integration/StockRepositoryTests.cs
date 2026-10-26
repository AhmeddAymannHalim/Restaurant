using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.Tests;

public class StockRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly StockRepository _sut;

    public StockRepositoryTests() => _sut = new StockRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<(int store, int product)> Seed()
    {
        var store = new Store { Code = "1", ArName = "Store", EnName = "S" };
        var product = new Product { Code = "1", ArName = "Product", EnName = "P" };
        _db.Context.Stores.Add(store);
        _db.Context.Products.Add(product);
        await _db.Context.SaveChangesAsync();
        return (store.Id, product.Id);
    }

    [Fact]
    public async Task BaseUnitId_CalledTwice_CreatesOneUnit()
    {
        var first = await _sut.BaseUnitIdAsync();
        var second = await _sut.BaseUnitIdAsync();

        Assert.Equal(first, second);
        Assert.Single(_db.Context.Units);
    }

    [Fact]
    public async Task Adjust_MissingRow_CreatesAndAdds()
    {
        var (store, product) = await Seed();

        var ok = await _sut.AdjustAsync(store, product, 5);

        Assert.True(ok);
        Assert.Equal(5, (await _sut.FindAsync(store, product)).Quantity);
    }

    [Fact]
    public async Task Adjust_MissingRowWithoutCreate_ReturnsFalse()
    {
        var (store, product) = await Seed();

        Assert.False(await _sut.AdjustAsync(store, product, 5, createIfMissing: false));
    }

    [Fact]
    public async Task Adjust_BelowZero_IsRejectedUnlessAllowed()
    {
        var (store, product) = await Seed();
        await _sut.AdjustAsync(store, product, 2);

        Assert.False(await _sut.AdjustAsync(store, product, -3));
        Assert.Equal(2, (await _sut.FindAsync(store, product)).Quantity);

        Assert.True(await _sut.AdjustAsync(store, product, -3, allowNegative: true));
        Assert.Equal(-1, (await _sut.FindAsync(store, product)).Quantity);
    }

    [Fact]
    public async Task Adjust_ToExactlyZero_IsAllowed()
    {
        var (store, product) = await Seed();
        await _sut.AdjustAsync(store, product, 2);

        Assert.True(await _sut.AdjustAsync(store, product, -2));
    }

    [Fact]
    public async Task Quantities_FiltersByStoreAndMapsProductId()
    {
        var (store, product) = await Seed();
        var other = new Store { Code = "2", ArName = "Store2", EnName = "S2" };
        _db.Context.Stores.Add(other);
        await _db.Context.SaveChangesAsync();
        await _sut.AdjustAsync(store, product, 4);
        await _sut.AdjustAsync(other.Id, product, 7);

        var all = await _sut.QuantitiesAsync();
        var one = await _sut.QuantitiesAsync(other.Id);

        Assert.Equal(2, all.Count);
        Assert.Equal(7, Assert.Single(one).Quantity);
        Assert.Equal(product, one[0].ProductId);
        Assert.Equal(7, _sut.FindQuantity(all, other.Id, product));
        Assert.Equal(0, _sut.FindQuantity(all, 999, product));
    }
}
