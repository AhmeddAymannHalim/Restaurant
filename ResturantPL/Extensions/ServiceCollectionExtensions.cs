using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using ResturantPL.Helper;
using ResturantPL.Services;

namespace ResturantPL.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "CorsPolicy";
    public const string StaffPolicy = "Staff";
    public const string ManagementPolicy = "Management";
    public const string AdminPolicy = "Admin";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        return services;
    }

    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddAuthenticationAndIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/Account/SignIn";
            options.AccessDeniedPath = "/Home/Error";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

        if (configuration.GetValue<bool>("Security:Relaxed"))
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(StaffPolicy, policy => policy.RequireAuthenticatedUser())
                .AddPolicy(ManagementPolicy, policy => policy.RequireAuthenticatedUser())
                .AddPolicy(AdminPolicy, policy => policy.RequireAuthenticatedUser());
        }
        else
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(StaffPolicy, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => !context.User.IsInRole("User")))
                .AddPolicy(ManagementPolicy, policy => policy.RequireRole("Admin", "Manager"))
                .AddPolicy(AdminPolicy, policy => policy.RequireRole("Admin"));
        }

        return services;
    }

    public static IServiceCollection AddCorsAndSession(this IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }
}
