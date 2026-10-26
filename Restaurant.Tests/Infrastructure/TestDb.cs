using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;

namespace Restaurant.Tests;

// SQLite cannot Sum/Average decimals, so the test database stores them as doubles.
public class SqliteContext : ApplicationDbContext
{
    public SqliteContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
        configurationBuilder.Properties<decimal?>().HaveConversion<double?>();
    }
}

// Makes the next save fail with the given exception, to reach the catch blocks of the repositories.
public class ForcedSaveFailure : SaveChangesInterceptor
{
    public Exception Failure { get; set; }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
        Failure is null ? base.SavingChangesAsync(eventData, result, cancellationToken) : throw Failure;
}

// Makes every database command fail with the given exception, to reach the catch blocks that guard a read or a save.
public class ForcedQueryFailure : DbCommandInterceptor
{
    public Exception Failure { get; set; }

    // how many commands may still run before the failure starts (to break a method half way, after its first reads)
    public int SkipFirst { get; set; }

    private bool ShouldFail()
    {
        if (Failure is null)
        {
            return false;
        }

        if (SkipFirst > 0)
        {
            SkipFirst--;
            return false;
        }

        return true;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) =>
        ShouldFail() ? throw Failure : base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result) =>
        ShouldFail() ? throw Failure : base.ReaderExecuting(command, eventData, result);

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
        ShouldFail() ? throw Failure : base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default) =>
        ShouldFail() ? throw Failure : base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
}

// One private in-memory SQLite database per test class instance, with foreign keys enforced like SQL Server.
public class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Context { get; }
    public IHttpContextAccessor Http { get; }
    public ForcedSaveFailure SaveFailure { get; } = new();
    public ForcedQueryFailure QueryFailure { get; } = new();

    // Show the real exception text instead of "ErrorWhileSaving" so a failing test explains itself.
    static TestDb() => Shared.Helpers.ErrorLog.ShowDetails = true;

    // invoiceAccounts: every invoice type gets an item account and a pay account, so its journal entry can be posted
    public TestDb(bool invoiceAccounts = false)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).AddInterceptors(SaveFailure, QueryFailure).Options;
        Context = new SqliteContext(options);
        Context.Database.EnsureCreated();

        Http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

        if (invoiceAccounts)
        {
            SeedInvoiceAccounts();
        }
    }

    public void SeedInvoiceAccounts()
    {
        var itemAccount = new Restaurant.DAL.Models.Account { Code = "TI", ArName = "Item", EnName = "Item", AccountType = Shared.Enums.AccountType.Normal, BalanceType = Shared.Enums.AccountBalanceType.Debit };
        var payAccount = new Restaurant.DAL.Models.Account { Code = "TP", ArName = "Cash", EnName = "Cash", AccountType = Shared.Enums.AccountType.Box, BalanceType = Shared.Enums.AccountBalanceType.Debit };
        var sellType = new Restaurant.DAL.Models.SellType { ArName = "Retail", EnName = "Retail", Show = true };
        Context.AddRange(itemAccount, payAccount, sellType);
        Context.SaveChanges();
        foreach (var billType in new[] { Shared.Enums.BillType.Purchases, Shared.Enums.BillType.PurchaseReturn, Shared.Enums.BillType.Sales, Shared.Enums.BillType.SalesReturn, Shared.Enums.BillType.InventoryDeficit, Shared.Enums.BillType.InventoryIncrease, Shared.Enums.BillType.OpeningBalance })
        {
            Context.BillSettings.Add(new Restaurant.DAL.Models.BillSetting
            {
                ArName = billType.ToString(), EnName = billType.ToString(), ArAbrvation = billType.ToString(), EnAbrvation = billType.ToString(),
                BillType = billType, EntryGenerate = Shared.Enums.EntryGenerate.AutomaticEntryCreated, EntryAutoPosted = true,
                StorageEffect = Shared.Enums.StorageEffect.AutoEffected, CurrencyRate = 1, DefaultPayType = Shared.Enums.PayType.Cash,
                SellTypeId = sellType.Id, AccountItemId = itemAccount.Id, AccountPayId = payAccount.Id
            });
        }
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
