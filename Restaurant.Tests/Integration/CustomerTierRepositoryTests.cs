using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.Tests;

public class CustomerTierRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly CustomerTierRepository _sut;

    public CustomerTierRepositoryTests()
    {
        _sut = new CustomerTierRepository(_db.Context, _db.Http);
    }

    public void Dispose() => _db.Dispose();

    private static CustomerTier Tier(int id, double percentage) => new()
    {
        Id = id,
        ArName = "Gold",
        EnName = "Gold",
        CustomerTierDiscounts = new List<CustomerTierDiscount> { new() { DiscountPercentage = percentage } }
    };

    [Fact]
    public async Task Upsert_New_SavesTheDiscountRow()
    {
        Assert.True((await _sut.UpsertCustomerTierAsync(Tier(0, 10))).Success);
        _db.Context.ChangeTracker.Clear();

        var tier = await _db.Context.CustomerTiers.AsNoTracking().SingleAsync();
        var loaded = await _sut.GetCustomerTierByIdAsync(tier.Id);
        Assert.Equal(10, loaded.CustomerTierDiscounts.Single().DiscountPercentage);
    }

    [Fact]
    public async Task Upsert_Edit_ChangesTheSameDiscountRow()
    {
        await _sut.UpsertCustomerTierAsync(Tier(0, 10));
        _db.Context.ChangeTracker.Clear();
        var id = (await _db.Context.CustomerTiers.AsNoTracking().SingleAsync()).Id;

        Assert.True((await _sut.UpsertCustomerTierAsync(Tier(id, 25))).Success);
        _db.Context.ChangeTracker.Clear();

        var discount = await _db.Context.CustomerTierDiscounts.AsNoTracking().SingleAsync();
        Assert.Equal(25, discount.DiscountPercentage);
        Assert.Equal(id, discount.CustomerTierId);
    }

    [Fact]
    public async Task Upsert_EditOfATierWithoutDiscount_AddsTheRow()
    {
        _db.Context.CustomerTiers.Add(new CustomerTier { Code = "1", ArName = "Old", EnName = "Old" });
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        var id = (await _db.Context.CustomerTiers.AsNoTracking().SingleAsync()).Id;

        Assert.True((await _sut.UpsertCustomerTierAsync(Tier(id, 15))).Success);
        _db.Context.ChangeTracker.Clear();

        Assert.Equal(15, (await _db.Context.CustomerTierDiscounts.AsNoTracking().SingleAsync()).DiscountPercentage);
    }

    [Fact]
    public async Task Delete_RemovesTheTierAndItsDiscount()
    {
        await _sut.UpsertCustomerTierAsync(Tier(0, 10));
        _db.Context.ChangeTracker.Clear();
        var id = (await _db.Context.CustomerTiers.AsNoTracking().SingleAsync()).Id;

        Assert.True((await _sut.DeleteCustomerTierAsync(id)).Success);
        _db.Context.ChangeTracker.Clear();

        Assert.False(await _db.Context.CustomerTiers.AnyAsync());
        Assert.False(await _db.Context.CustomerTierDiscounts.AnyAsync());
    }
}
