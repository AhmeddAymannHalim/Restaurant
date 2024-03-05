using AutoMapper;
using Service.Repositories;
using Service.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResturantPL.Helper;
using StackExchange.Redis;
using Service.IRepositories;
using Restaurant.Data;
using Restaurant.Data.Models;
using Restaurant.Data.SeedData;
using Rotativa.AspNetCore;


namespace ResturantPL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           // CreateHostBuilder(args).Build().Run();


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


            builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
            {
                return ConnectionMultiplexer.Connect(builder.Configuration["RedisConnection"]);
            });

            builder.Services.AddAutoMapper(typeof(Profiler));
            builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            //builder.Services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));
            //builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


            /*****authentication and identity ********************/

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            }
            )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;// to force it to check
            })
                .AddCookie(Option =>
                {
                    Option.LoginPath = "/Account/SignIn";
                    Option.AccessDeniedPath = "/Home/Error";
                    Option.Cookie.HttpOnly = true;
                    Option.ExpireTimeSpan = TimeSpan.FromDays(7);
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

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;

            var roleManger = scopeFactory.GetRequiredService<RoleManager<IdentityRole>>();
            var userManger = scopeFactory.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scopeFactory.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            // await SeedRestaurantData.SeedAsync(roleManger);
            // await SeedData.SeedDataAsync(dbContext, userManger);

            DataSeeder.SeedData(dbContext);

            app.Run();
        }

    }
}