using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class PhoneRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static List<CustomerPhone> Phones(params string[] numbers)
    {
        return numbers.Select(a => new CustomerPhone { PhoneNumber = a }).ToList();
    }

    // ---------- CleanNumbers ----------

    [Fact]
    public void CleanNumbers_TrimsAndKeepsTheOrder()
    {
        using var context = CreateContext();
        var repository = new PhoneRepository(context);

        var numbers = repository.CleanNumbers("0100", Phones(" 0111 ", "0122"));

        Assert.Equal(new[] { "0111", "0122" }, numbers.ToArray());
    }

    [Fact]
    public void CleanNumbers_DropsEmptyOnesAndTheMainNumber()
    {
        using var context = CreateContext();
        var repository = new PhoneRepository(context);

        var numbers = repository.CleanNumbers("0100", Phones("", "   ", "0100", " 0100 ", "0111"));

        Assert.Equal(new[] { "0111" }, numbers.ToArray());
    }

    [Fact]
    public void CleanNumbers_DropsRepeatedNumbers()
    {
        using var context = CreateContext();
        var repository = new PhoneRepository(context);

        var numbers = repository.CleanNumbers("0100", Phones("0111", "0111", "0122"));

        Assert.Equal(new[] { "0111", "0122" }, numbers.ToArray());
    }

    [Fact]
    public void CleanNumbers_NoExtraNumbers_GivesAnEmptyList()
    {
        using var context = CreateContext();
        var repository = new PhoneRepository(context);

        Assert.Empty(repository.CleanNumbers("0100", null));
    }

    // ---------- Sync ----------

    [Fact]
    public async Task SyncCustomerPhones_RemovesOldAddsNewAndKeepsTheRest()
    {
        // Arrange: the customer has A and B, the screen now sends B and C
        using var context = CreateContext();
        var customer = new Customer { Phones = Phones("A", "B") };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        var repository = new PhoneRepository(context);

        // Act
        repository.SyncCustomerPhones(customer.Phones, new List<string> { "B", "C" });
        await context.SaveChangesAsync();

        // Assert
        var saved = context.CustomerPhones.Select(a => a.PhoneNumber).OrderBy(a => a).ToList();
        Assert.Equal(new[] { "B", "C" }, saved.ToArray());
    }

    [Fact]
    public async Task SyncCustomerPhones_EmptyWantedList_RemovesEverything()
    {
        using var context = CreateContext();
        var customer = new Customer { Phones = Phones("A", "B") };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        var repository = new PhoneRepository(context);

        repository.SyncCustomerPhones(customer.Phones, new List<string>());
        await context.SaveChangesAsync();

        Assert.Empty(context.CustomerPhones);
    }

    [Fact]
    public async Task SyncStorePhones_RemovesOldAndAddsNew()
    {
        using var context = CreateContext();
        var store = new Store { ArName = "s", EnName = "s", Phones = new List<StorePhone> { new StorePhone { PhoneNumber = "A" } } };
        context.Stores.Add(store);
        await context.SaveChangesAsync();
        var repository = new PhoneRepository(context);

        repository.SyncStorePhones(store.Phones, new List<string> { "B" });
        await context.SaveChangesAsync();

        Assert.Equal(new[] { "B" }, context.StorePhones.Select(a => a.PhoneNumber).ToArray());
    }

    [Fact]
    public async Task SyncSupplierPhones_RemovesOldAndAddsNew()
    {
        using var context = CreateContext();
        var supplier = new Supplier { Phones = new List<SupplierPhone> { new SupplierPhone { PhoneNumber = "A" } } };
        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();
        var repository = new PhoneRepository(context);

        repository.SyncSupplierPhones(supplier.Phones, new List<string> { "A", "B" });
        await context.SaveChangesAsync();

        Assert.Equal(new[] { "A", "B" }, context.SupplierPhones.Select(a => a.PhoneNumber).OrderBy(a => a).ToArray());
    }
}
