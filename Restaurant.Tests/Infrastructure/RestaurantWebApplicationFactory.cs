using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.SeedData;

namespace Restaurant.Tests;

// The real app (same Program.cs, routes, filters, localization and views) in memory on SQLite, signed in as an Admin.
public sealed class RestaurantWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string Scheme = "Test";
    public static string AdminId { get; private set; }

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public RestaurantWebApplicationFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddScoped(_ => new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
            services.AddScoped<ApplicationDbContext>(provider => new SqliteContext(provider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()));

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = Scheme;
                    options.DefaultChallengeScheme = Scheme;
                    options.DefaultScheme = Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(Scheme, _ => { });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        DataSeeder.SeedCountries(context).GetAwaiter().GetResult();
        DataSeeder.SeedGovs(context).GetAwaiter().GetResult();
        DataSeeder.SeedTowns(context).GetAwaiter().GetResult();
        DataSeeder.SeedApplicationSettings(context, "1234").GetAwaiter().GetResult();
        DataSeeder.SeedPrinterSettings(context).GetAwaiter().GetResult();
        DataSeeder.SeedAdminRoleAndUsers(userManager, roleManager, true, "@Qwe12").GetAwaiter().GetResult();
        AdminId = userManager.GetUsersInRoleAsync("Admin").GetAwaiter().GetResult().First().Id;

        return host;
    }

    public HttpClient CreateClient(string culture)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Culture=c={culture}|uic={culture}");
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, RestaurantWebApplicationFactory.AdminId),
            new Claim(ClaimTypes.Name, "Halim"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, RestaurantWebApplicationFactory.Scheme));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, RestaurantWebApplicationFactory.Scheme)));
    }
}
