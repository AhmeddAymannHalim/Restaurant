using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

// One private in-memory SQLite database per test class instance, with foreign keys enforced like SQL Server.
public class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Context { get; }
    public IHttpContextAccessor Http { get; }

    // Show the real exception text instead of "ErrorWhileSaving" so a failing test explains itself.
    static TestDb() => Shared.Helpers.ErrorLog.ShowDetails = true;

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        Context = new SqliteContext(options);
        Context.Database.EnsureCreated();

        Http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
