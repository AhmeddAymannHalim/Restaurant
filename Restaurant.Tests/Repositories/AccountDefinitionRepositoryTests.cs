using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Restaurant.Tests.Repositories;

public class AccountDefinitionRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Account BuildAccount(string code, AccountType type, AccountBalanceType balanceType,
        decimal debit = 0, decimal credit = 0, AccountDetails details = null)
    {
        return new Account
        {
            Code = code,
            ArName = "حساب " + code,
            EnName = "Account " + code,
            AccountType = type,
            BalanceType = balanceType,
            DebitBalance = debit,
            CreditBalance = credit,
            AccDate = DateTime.Today,
            AccCheckDate = DateTime.Today,
            AccountDetails = details
        };
    }

    [Fact]
    public async Task GetPagedAccountsAsync_NoFilters_ReturnsAllAccountsPaged()
    {
        // Arrange
        using var context = CreateContext();
        context.Accounts.AddRange(
            BuildAccount("1010", AccountType.Box, AccountBalanceType.Debit, debit: 100),
            BuildAccount("1020", AccountType.Normal, AccountBalanceType.Debit, debit: 50));
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetPagedAccountsAsync(new PageRequest(), assetsOnly: false);

        // Assert
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetPagedAccountsAsync_AssetsOnly_ReturnsOnlyAccountsWithAccountDetails()
    {
        // Arrange
        using var context = CreateContext();
        var assetDetails = new AccountDetails { BuyValue = 1000 };
        context.Accounts.AddRange(
            BuildAccount("1500", AccountType.Normal, AccountBalanceType.Debit, debit: 1000, details: assetDetails),
            BuildAccount("1020", AccountType.Normal, AccountBalanceType.Debit, debit: 50));
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetPagedAccountsAsync(new PageRequest(), assetsOnly: true);

        // Assert
        Assert.Equal(1, result.TotalRecords);
        Assert.Equal("1500", result.Data.Single().Code);
        Assert.Equal(AccountKind.Asset, result.Data.Single().Kind);
    }

    [Fact]
    public async Task GetPagedAccountsAsync_DebitAccount_ComputesBalanceAsDebitMinusCredit()
    {
        // Arrange
        using var context = CreateContext();
        context.Accounts.Add(BuildAccount("1030", AccountType.Normal, AccountBalanceType.Debit, debit: 300, credit: 100));
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetPagedAccountsAsync(new PageRequest(), assetsOnly: false);

        // Assert
        var account = result.Data.Single();
        Assert.Equal(200, account.Balance);
    }

    [Fact]
    public async Task GetPagedAccountsAsync_CreditAccount_ComputesBalanceAsCreditMinusDebit()
    {
        // Arrange
        using var context = CreateContext();
        context.Accounts.Add(BuildAccount("2010", AccountType.Normal, AccountBalanceType.Credit, debit: 50, credit: 400));
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetPagedAccountsAsync(new PageRequest(), assetsOnly: false);

        // Assert
        var account = result.Data.Single();
        Assert.Equal(350, account.Balance);
    }

    [Theory]
    [InlineData(AccountType.Box, AccountKind.Safe)]
    [InlineData(AccountType.Bank, AccountKind.Bank)]
    [InlineData(AccountType.Customer, AccountKind.Owned)]
    public async Task GetPagedAccountsAsync_AccountTypeDrivesKindClassification(AccountType accountType, AccountKind expectedKind)
    {
        // Arrange
        using var context = CreateContext();
        context.Accounts.Add(BuildAccount("9999", accountType, AccountBalanceType.Debit, debit: 10));
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetPagedAccountsAsync(new PageRequest(), assetsOnly: false);

        // Assert
        Assert.Equal(expectedKind, result.Data.Single().Kind);
    }

    [Fact]
    public async Task GetAccountByIdAsync_ExistingAccount_ReturnsViewModelWithComputedBalance()
    {
        // Arrange
        using var context = CreateContext();
        var account = BuildAccount("1040", AccountType.Normal, AccountBalanceType.Debit, debit: 500, credit: 120);
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetAccountByIdAsync(account.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1040", result.Code);
        Assert.Equal(380, result.Balance);
        Assert.Equal(AccountKind.GeneralLedger, result.Kind);
    }

    [Fact]
    public async Task GetAccountByIdAsync_AccountDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new AccountDefinitionRepository(context);

        // Act
        var result = await repository.GetAccountByIdAsync(999);

        // Assert
        Assert.Null(result);
    }
}
