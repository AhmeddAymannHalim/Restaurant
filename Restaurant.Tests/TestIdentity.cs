using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.DAL;
using Restaurant.DAL.Models;

namespace Restaurant.Tests;

// The real UserManager / RoleManager / SignInManager, set up like Program.cs, on the test database.
public class TestIdentity : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public UserManager<ApplicationUser> Users { get; }
    public RoleManager<IdentityRole> Roles { get; }
    public SignInManager<ApplicationUser> SignIn { get; }

    public TestIdentity(ApplicationDbContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddSingleton(context);
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        Users = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        SignIn = _scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }
}
