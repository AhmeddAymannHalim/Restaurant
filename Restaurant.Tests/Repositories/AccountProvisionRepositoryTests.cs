using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class AccountProvisionRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Account> AddParentAsync(ApplicationDbContext context, string code, int level = 2)
    {
        var parent = new Account { Code = code, ArName = "parent", EnName = "parent", Level = level, AccountType = AccountType.Collective, BalanceType = AccountBalanceType.Debit };
        context.Accounts.Add(parent);
        await context.SaveChangesAsync();
        return parent;
    }

    [Fact]
    public async Task CreateAsync_FirstChild_GetsTheParentCodePlusOne()
    {
        using var context = CreateContext();
        await AddParentAsync(context, "1100");
        var repository = new AccountProvisionRepository(context);

        var account = await repository.CreateAsync("عميل", "Customer", AccountType.Customer, "1100");

        Assert.Equal("11000001", account.Code);
    }

    [Fact]
    public async Task CreateAsync_NextChild_GetsTheNextNumber()
    {
        using var context = CreateContext();
        await AddParentAsync(context, "1100");
        var repository = new AccountProvisionRepository(context);
        await repository.CreateAsync("a", "a", AccountType.Customer, "1100");
        await context.SaveChangesAsync();

        var second = await repository.CreateAsync("b", "b", AccountType.Customer, "1100");

        Assert.Equal("11000002", second.Code);
    }

    [Fact]
    public async Task CreateAsync_ChildCodeWithLetters_IsIgnoredWhenCounting()
    {
        // Arrange: a child whose code does not end in a number does not move the counter
        using var context = CreateContext();
        var parent = await AddParentAsync(context, "1100");
        context.Accounts.Add(new Account { Code = "1100ABCD", ArName = "x", EnName = "x", ParentAccountId = parent.Id });
        context.Accounts.Add(new Account { Code = "11000007", ArName = "y", EnName = "y", ParentAccountId = parent.Id });
        await context.SaveChangesAsync();
        var repository = new AccountProvisionRepository(context);

        var account = await repository.CreateAsync("n", "n", AccountType.Customer, "1100");

        Assert.Equal("11000008", account.Code);
    }

    [Fact]
    public async Task CreateAsync_NewAccount_IsLinkedToItsParentOneLevelDown()
    {
        using var context = CreateContext();
        var parent = await AddParentAsync(context, "1100", level: 3);
        var repository = new AccountProvisionRepository(context);

        var account = await repository.CreateAsync("n", "n", AccountType.Customer, "1100");

        Assert.Equal(parent.Id, account.ParentAccountId);
        Assert.Equal(4, account.Level);
    }

    [Fact]
    public async Task CreateAsync_CountsTheNewSubAccountOnTheParent()
    {
        using var context = CreateContext();
        var parent = await AddParentAsync(context, "1100");
        var repository = new AccountProvisionRepository(context);

        await repository.CreateAsync("a", "a", AccountType.Customer, "1100");
        await repository.CreateAsync("b", "b", AccountType.Customer, "1100");

        Assert.Equal(2, parent.NumberOfSubAccounts);
    }

    [Theory]
    [InlineData(AccountType.Customer, AccountBalanceType.Debit)]
    [InlineData(AccountType.Box, AccountBalanceType.Debit)]
    [InlineData(AccountType.Bank, AccountBalanceType.Debit)]
    [InlineData(AccountType.Supplier, AccountBalanceType.Credit)]
    [InlineData(AccountType.Employee, AccountBalanceType.Credit)]
    public async Task CreateAsync_BalanceSideFollowsTheAccountType(AccountType type, AccountBalanceType expected)
    {
        using var context = CreateContext();
        await AddParentAsync(context, "1100");
        var repository = new AccountProvisionRepository(context);

        var account = await repository.CreateAsync("n", "n", type, "1100");

        Assert.Equal(expected, account.BalanceType);
    }

    [Fact]
    public async Task CreateAsync_OpeningBalance_IsPutOnTheNormalSideOfTheAccount()
    {
        using var context = CreateContext();
        await AddParentAsync(context, "1100");
        var repository = new AccountProvisionRepository(context);

        var customer = await repository.CreateAsync("c", "c", AccountType.Customer, "1100", 500);
        var supplier = await repository.CreateAsync("s", "s", AccountType.Supplier, "1100", 300);

        Assert.Equal(500, customer.DebitBalance);
        Assert.Equal(0, customer.CreditBalance);
        Assert.Equal(300, supplier.CreditBalance);
        Assert.Equal(0, supplier.DebitBalance);
    }
}
