using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Commons;
using Shared.Enums;

namespace Restaurant.Tests;

public class JournalEntryRepositoryTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly JournalEntryRepository _sut;

    public JournalEntryRepositoryTests() => _sut = new JournalEntryRepository(_db.Context);

    public void Dispose() => _db.Dispose();

    private async Task<Account> SeedAccount(string code, AccountType type = AccountType.Normal, AccountBalanceType balance = AccountBalanceType.Debit)
    {
        var account = new Account { Code = code, ArName = "Account " + code, EnName = "Account " + code, AccountType = type, BalanceType = balance };
        _db.Context.Accounts.Add(account);
        await _db.Context.SaveChangesAsync();
        return account;
    }

    [Fact]
    public async Task GetNextEntryNumber_Empty_ReturnsOne()
    {
        Assert.Equal("1", await _sut.GetNextEntryNumberAsync());
    }

    [Fact]
    public async Task PostSourceEntry_Balanced_CreatesEntryAndMovesNormalAccounts()
    {
        var cash = await SeedAccount("1", balance: AccountBalanceType.Debit);
        var income = await SeedAccount("2", balance: AccountBalanceType.Credit);

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 7, new DateTime(2030, 1, 1), "test",
            new[] { new JournalLineRequest(cash.Id, 100, 0), new JournalLineRequest(income.Id, 0, 100) });

        Assert.True(result.Success, result.Message);
        Assert.Equal("1", result.Data);
        var entry = await _db.Context.EntryMasters.Include(a => a.EntryDetails).SingleAsync();
        Assert.Equal("Manual:7", entry.CollectEntryCode);
        Assert.Equal(100, entry.TotalBalance);
        Assert.Equal(2, entry.EntryDetails.Count);
        Assert.Equal(100, (await _db.Context.Accounts.FindAsync(cash.Id)).DebitBalance);
        Assert.Equal(100, (await _db.Context.Accounts.FindAsync(income.Id)).CreditBalance);
    }

    [Fact]
    public async Task PostSourceEntry_Unbalanced_IsRejectedAndSavesNothing()
    {
        var cash = await SeedAccount("1");
        var income = await SeedAccount("2", balance: AccountBalanceType.Credit);

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, 100, 0), new JournalLineRequest(income.Id, 0, 90) });

        Assert.False(result.Success);
        Assert.Equal("JournalNotBalanced", result.Message);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task PostSourceEntry_LineWithBothSides_IsRejected()
    {
        var cash = await SeedAccount("1");

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, 100, 100) });

        Assert.Equal("JournalLineInvalid", result.Message);
    }

    [Fact]
    public async Task PostSourceEntry_NegativeAmount_IsRejected()
    {
        var cash = await SeedAccount("1");
        var other = await SeedAccount("2");

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, -50, 0), new JournalLineRequest(other.Id, 0, -50) });

        Assert.False(result.Success);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task PostSourceEntry_NoLines_IsSuccessWithoutEntry()
    {
        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x", new JournalLineRequest[0]);

        Assert.True(result.Success);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task PostSourceEntry_UnknownAccount_ReturnsRecordNotFound()
    {
        var cash = await SeedAccount("1");

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, 10, 0), new JournalLineRequest(999, 0, 10) });

        Assert.Equal("RecordNotFound", result.Message);
    }

    [Fact]
    public async Task PostSourceEntry_MainAccountLineWithoutSetting_PostsNothing()
    {
        var customer = await SeedAccount("1", AccountType.Customer);

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.CustomerPaymentVoucher, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(customer.Id, 0, 50), new JournalLineRequest(null, 50, 0) });

        Assert.True(result.Success);
        Assert.Empty(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task PostSourceEntry_CustomerAccount_BalanceIsLeftToTheCaller()
    {
        var customer = await SeedAccount("1", AccountType.Customer);
        var box = await SeedAccount("2", AccountType.Box);
        var setting = new EntrySetting { EntryType = EntryType.CustomerPayment, ArName = "a", EnName = "b", AccountId = box.Id };
        _db.Context.EntrySettings.Add(setting);
        await _db.Context.SaveChangesAsync();

        var result = await _sut.PostSourceEntryAsync(JournalSourceType.CustomerPaymentVoucher, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(customer.Id, 0, 50), new JournalLineRequest(null, 50, 0) });

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, (await _db.Context.Accounts.FindAsync(customer.Id)).CreditBalance);
        Assert.Equal(50, (await _db.Context.Accounts.FindAsync(box.Id)).DebitBalance);
    }

    [Fact]
    public async Task PostSourceEntry_SecondEntry_GetsNextNumber()
    {
        var a = await SeedAccount("1");
        var b = await SeedAccount("2", balance: AccountBalanceType.Credit);
        var lines = new[] { new JournalLineRequest(a.Id, 5, 0), new JournalLineRequest(b.Id, 0, 5) };

        await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x", lines);
        var second = await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 2, DateTime.Today, "x", lines);

        Assert.Equal("2", second.Data);
    }

    [Fact]
    public async Task RemoveSourceEntries_GivesBackBalancesAndDeletesEntry()
    {
        var cash = await SeedAccount("1");
        var income = await SeedAccount("2", balance: AccountBalanceType.Credit);
        await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 9, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, 40, 0), new JournalLineRequest(income.Id, 0, 40) });

        await _sut.RemoveSourceEntriesAsync(JournalSourceType.Manual, 9);
        await _db.Context.SaveChangesAsync();

        Assert.Empty(_db.Context.EntryMasters);
        Assert.Empty(_db.Context.EntryDetails);
        var after = await _db.Context.Accounts.AsNoTracking().SingleAsync(a => a.Id == cash.Id);
        Assert.Equal(0, after.BalanceValue);
    }

    [Fact]
    public async Task RemoveSourceEntries_OtherSource_IsUntouched()
    {
        var cash = await SeedAccount("1");
        var income = await SeedAccount("2", balance: AccountBalanceType.Credit);
        await _sut.PostSourceEntryAsync(JournalSourceType.Manual, 1, DateTime.Today, "x",
            new[] { new JournalLineRequest(cash.Id, 40, 0), new JournalLineRequest(income.Id, 0, 40) });

        await _sut.RemoveSourceEntriesAsync(JournalSourceType.Manual, 2);
        await _db.Context.SaveChangesAsync();

        Assert.Single(_db.Context.EntryMasters);
    }

    [Fact]
    public async Task PostOpeningBalance_ZeroDelta_DoesNothing()
    {
        var customer = await SeedAccount("1", AccountType.Customer);

        var result = await _sut.PostOpeningBalanceAsync(customer, 0);

        Assert.True(result.Success);
        Assert.Empty(_db.Context.EntryMasters);
    }
}
