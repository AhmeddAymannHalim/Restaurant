using Service.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;
using Restaurant.Data;
using Restaurant.Data.Models;
using Restaurant.Data.SeedData;
using Rotativa.AspNetCore;
using ResturantPL.Hubs;
using System.Security.Claims;

namespace ResturantPL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });


            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 44373;
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            });

            builder.Services.AddAutoMapper(typeof(Profiler));
            builder.Services.AddSignalR();
            builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

            // Identity setup
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            
            builder.Services.AddAuthentication(options =>
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



            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseSession();
            app.UseRotativa();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<OrderHub>("/OrderHub");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;
            var roleManger = scopeFactory.GetRequiredService<RoleManager<IdentityRole>>();
            var userManger = scopeFactory.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scopeFactory.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            await DataSeeder.SeedData(dbContext, userManger, roleManger);

            app.Run();
        }
    }
}
