using Microsoft.AspNetCore.Identity;
using Restaurant.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.SeedData
{
    public static class DataSeeder
    {
        public static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedJobs(context);
            await SeedIndexSettings(context);
            await SeedTables(context);
            await SeedSalaryTypes(context);
            await SeedUnitPrices(context);
            await SeedApplicationSettings(context);
            await SeedPrinterSettings(context);
            await SeedAdminRoleAndUsers(userManager, roleManager);
        }

        private static async Task SeedJobs(ApplicationDbContext context)
        {
            if (!context.Jobs.Any())
            {
                var jobs = new[]
                {
                    new Job { Name = "كاشير", Code = "7456" },
                    new Job { Name = "طيار", Code = "1425" },
                    new Job { Name = "امين مخزن", Code = "5241" },
                    new Job { Name = "جرسون", Code = "9841" },
                    new Job { Name = "طباخ", Code = "3452" },
                    new Job { Name = "نادل الصالة", Code = "4245" },
                };
                context.Jobs.AddRange(jobs);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedIndexSettings(ApplicationDbContext context)
        {
            if (!context.IndexSettings.Any())
            {
                var indexes = new[]
                {
                    new IndexSettings { Name = "الاكلات الشهية", Description = "جميع الاطعمة و المشويات", Email = "di.hamdan55@gmail.com", StartDay = "السبت", EndDay = "السبت", Phone = "01010164599", Locaion = "القاهرة رمسيس شارع الفجالة", StartHour = "9", EndHour = "12", ImageUrl = null, VedioUrl = null }
                };
                context.IndexSettings.AddRange(indexes);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTables(ApplicationDbContext context)
        {
            if (!context.Tables.Any())
            {
                var tables = new[]
                {
                    new Table { Name = "طاولة رقم 1 ", IsPending = false, NumOfPersons = 4},
                    new Table { Name = "طاولة رقم 2 ", IsPending = false, NumOfPersons = 6},
                    new Table { Name = "طاولة رقم 3 ", IsPending = false, NumOfPersons = 8},
                    new Table { Name = "طاولة رقم 4 ", IsPending = false, NumOfPersons = 3},
                    new Table { Name = "طاولة رقم 5 ", IsPending = false, NumOfPersons = 2},
                    new Table { Name = "طاولة رقم 6 ", IsPending = false, NumOfPersons = 4},
                    new Table { Name = "طاولة رقم 7 ", IsPending = false, NumOfPersons = 3},
                };
                context.Tables.AddRange(tables);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSalaryTypes(ApplicationDbContext context)
        {
            if (!context.SalaryTypes.Any())
            {
                var types = new[]
                {
                    new SalaryType { Type = "سنوي"},
                    new SalaryType { Type = "شهري"},
                    new SalaryType { Type = "اسبوعي"},
                    new SalaryType { Type = "يومي"},
                    new SalaryType { Type = "تدريب مجاني"},
                };
                context.SalaryTypes.AddRange(types);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUnitPrices(ApplicationDbContext context)
        {
            if (!context.UnitPrices.Any())
            {
                var units = new[]
                {
                    new UnitPrice { Name = "كيلو"},
                    new UnitPrice { Name = "جرام"},
                    new UnitPrice { Name = "طن"},
                    new UnitPrice { Name = "علبة"},
                    new UnitPrice { Name = "لتر"},
                    new UnitPrice { Name = "كيس"},
                    new UnitPrice { Name = "قطعة"},
                    new UnitPrice { Name = "زجاجة"},
                };
                context.UnitPrices.AddRange(units);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedApplicationSettings(ApplicationDbContext context)
        {
            if (!context.ApplicationSettings.Any())
            {
                var setting = new[]
                {
                    new ApplicationSetting { Password = "@Qwe12", OrderTax = 5, OrderDiscount = 1, OrderService = 10, PrintAfterOrder = false, PrintForTakeawayOrder = false, NumberOfCustomerPrint = 1, NumberOfKitchenPrint = 1, ShowTotalShift = true }
                };
                context.ApplicationSettings.AddRange(setting);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPrinterSettings(ApplicationDbContext context)
        {
            if (!context.PrinterSettings.Any())
            {
                var setting = new[]
                {
                    new PrinterSetting { PrinterForAllSections = "Microsoft Print to PDF", PrinterForCashier = "Microsoft Print to PDF", PrinterForEachSection = "Microsoft Print to PDF"}
                };
                context.PrinterSettings.AddRange(setting);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAdminRoleAndUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Manager"))
            {
                await roleManager.CreateAsync(new IdentityRole("Manager"));
            }
            if (!await roleManager.RoleExistsAsync("كاشير"))
            {
                await roleManager.CreateAsync(new IdentityRole("كاشير"));
            }

            var users = new[]
            {
                new ApplicationUser { FirstName = "ضيف الله", LastName = "حمدان" ,UserName = "Daif", Email = "daif@gmail.com", PhoneNumber = "01010164599" , UserType = "Admin"},
                new ApplicationUser { FirstName = "محمود", LastName = "اسماعيل" ,UserName = "Mahmoud", Email = "mahmoud@gmail.com", PhoneNumber = "01154809509", UserType = "Admin" }
            };

            foreach (var user in users)
            {
                var existingUser = await userManager.FindByNameAsync(user.UserName);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(user, "@Qwe12");
                    existingUser = user;
                }
                if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(existingUser, "Admin");
                }
            }
        }


    }


}
