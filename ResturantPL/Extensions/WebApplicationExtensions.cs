using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.SeedData;
using ResturantPL.Hubs;
using Rotativa.AspNetCore;

namespace ResturantPL.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseWebPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
        app.UseSession();
        app.UseRotativa();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<OrderHub>("/OrderHub");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }

    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await PrepareEmployeeCityBackfillAsync(dbContext);
        await dbContext.Database.MigrateAsync();
        await RemoveUnusedCityPlaceholderAsync(dbContext);

        await DataSeeder.SeedData(
            dbContext,
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>());
    }

    private const string CityFkMigration = "20261013171012_EmployeeAndUserCityFk";
    private const string BeforeCityFkMigration = "20261013155553_MergePhoneMobileAndLocations";

    private static async Task PrepareEmployeeCityBackfillAsync(ApplicationDbContext db)
    {
        var pending = await db.Database.GetPendingMigrationsAsync();
        if (!pending.Contains(CityFkMigration)) return;

        await db.Database.GetService<IMigrator>().MigrateAsync(BeforeCityFkMigration);

        await db.Database.ExecuteSqlRawAsync(@"
            IF EXISTS (SELECT 1 FROM Employees) AND NOT EXISTS (SELECT 1 FROM Cities WHERE Id = 0)
            BEGIN
                DECLARE @CountryId INT = (SELECT TOP 1 Id FROM Countries ORDER BY Id);
                IF @CountryId IS NULL
                BEGIN
                    INSERT INTO Countries (ArName, EnName) VALUES (N'غير محدد', N'Unknown');
                    SET @CountryId = SCOPE_IDENTITY();
                END

                DECLARE @GovId INT = (SELECT TOP 1 Id FROM Governorates ORDER BY Id);
                IF @GovId IS NULL
                BEGIN
                    INSERT INTO Governorates (ArName, EnName, CountryId) VALUES (N'غير محدد', N'Unknown', @CountryId);
                    SET @GovId = SCOPE_IDENTITY();
                END

                SET IDENTITY_INSERT Cities ON;
                INSERT INTO Cities (Id, ArName, EnName, GovernorateId) VALUES (0, N'غير محدد', N'Unknown', @GovId);
                SET IDENTITY_INSERT Cities OFF;
            END");
    }

    private static Task RemoveUnusedCityPlaceholderAsync(ApplicationDbContext db) =>
        db.Database.ExecuteSqlRawAsync(@"
            DELETE FROM Cities WHERE Id = 0
              AND NOT EXISTS (SELECT 1 FROM Employees WHERE CityId = 0)
              AND NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE CityId = 0)
              AND NOT EXISTS (SELECT 1 FROM Customers WHERE CityId = 0)");
}
