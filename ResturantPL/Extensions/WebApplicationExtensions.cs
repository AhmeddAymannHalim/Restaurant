using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
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
        await dbContext.Database.MigrateAsync();

        await DataSeeder.SeedData(
            dbContext,
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>());
    }
}
