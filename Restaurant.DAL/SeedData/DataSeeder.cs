using Restaurant.DAL.Helpers;
using Microsoft.AspNetCore.Identity;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.SeedData
{
    public static class DataSeeder
    {
        public static async Task SeedData(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await SeedJobs(context);
            await SeedIndexSettings(context);
            await SeedTables(context);
            await SeedSalaryTypes(context);
            await SeedUnitPrices(context);
            await SeedProductCategories(context);
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
                    new Job
                    {
                        ArName = "كاشير",
                        EnName = "Cashier",
                        Code = "1"
                    },
                    new Job
                    {
                        ArName = "طيار",
                        EnName = "Delivery Driver",
                        Code = "2"
                    },
                    new Job
                    {
                        ArName = "امين مخزن",
                        EnName = "Storekeeper",
                        Code = "3"
                    },
                    new Job
                    {
                        ArName = "جرسون",
                        EnName = "Waiter",
                        Code = "4"
                    },
                    new Job
                    {
                        ArName = "طباخ",
                        EnName = "Cook",
                        Code = "5"
                    },
                    new Job
                    {
                        ArName = "نادل الصالة",
                        EnName = "Hall Waiter",
                        Code = "6"
                    },
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
                    new IndexSettings
                    {
                        ArName = "الاكلات الشهية",
                        EnName = "Delicious Meals",
                        ArDescription = "جميع الاطعمة و المشويات",
                        EnDescription = "All kinds of food and grills",
                        Email = "di.hamdan55@gmail.com",
                        StartDay = "Saturday",
                        EndDay = "Friday",
                        Phone = "01010164599",
                        ArLocation = "القاهرة رمسيس شارع الفجالة",
                        EnLocation = "Cairo, Ramses, El-Fagala Street",
                        StartHour = "9",
                        EndHour = "12",
                        ImageUrl = null,
                        VideoUrl = null
                    }
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
                    new Table
                    {
                        ArName = "طاولة رقم 1 ",
                        EnName = "Table No. 1 ",
                        IsPending = false,
                        NumOfPersons = 4
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 2 ",
                        EnName = "Table No. 2 ",
                        IsPending = false,
                        NumOfPersons = 6
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 3 ",
                        EnName = "Table No. 3 ",
                        IsPending = false,
                        NumOfPersons = 8
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 4 ",
                        EnName = "Table No. 4 ",
                        IsPending = false,
                        NumOfPersons = 3
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 5 ",
                        EnName = "Table No. 5 ",
                        IsPending = false,
                        NumOfPersons = 2
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 6 ",
                        EnName = "Table No. 6 ",
                        IsPending = false,
                        NumOfPersons = 4
                    },
                    new Table
                    {
                        ArName = "طاولة رقم 7 ",
                        EnName = "Table No. 7 ",
                        IsPending = false,
                        NumOfPersons = 3
                    },
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
                    new SalaryType
                    {
                        ArName = "سنوي",
                        EnName = "Yearly"
                    },
                    new SalaryType
                    {
                        ArName = "شهري",
                        EnName = "Monthly"
                    },
                    new SalaryType
                    {
                        ArName = "اسبوعي",
                        EnName = "Weekly"
                    },
                    new SalaryType
                    {
                        ArName = "يومي",
                        EnName = "Daily"
                    },
                    new SalaryType
                    {
                        ArName = "تدريب مجاني",
                        EnName = "Free Training"
                    },
                };
                context.SalaryTypes.AddRange(types);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProductCategories(ApplicationDbContext context)
        {
            if (!context.ProductCategories.Any())
            {
                var categories = new[]
                {
                    new ProductCategory
                    {
                        ArName = "خضروات وفواكه",
                        EnName = "Vegetables and fruits"
                    },
                    new ProductCategory
                    {
                        ArName = "لحوم ودواجن",
                        EnName = "Meat and poultry"
                    },
                    new ProductCategory
                    {
                        ArName = "ألبان وأجبان",
                        EnName = "Dairy"
                    },
                    new ProductCategory
                    {
                        ArName = "مخبوزات ودقيق",
                        EnName = "Bakery and flour"
                    },
                    new ProductCategory
                    {
                        ArName = "مشروبات",
                        EnName = "Drinks"
                    },
                    new ProductCategory
                    {
                        ArName = "أخرى",
                        EnName = "Other"
                    }
                };
                context.ProductCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUnitPrices(ApplicationDbContext context)
        {
            if (!context.UnitPrices.Any())
            {
                var units = new[]
                {
                    new UnitPrice
                    {
                        ArName = "كيلو",
                        EnName = "Kilogram"
                    },
                    new UnitPrice
                    {
                        ArName = "جرام",
                        EnName = "Gram"
                    },
                    new UnitPrice
                    {
                        ArName = "طن",
                        EnName = "Ton"
                    },
                    new UnitPrice
                    {
                        ArName = "علبة",
                        EnName = "Box"
                    },
                    new UnitPrice
                    {
                        ArName = "لتر",
                        EnName = "Liter"
                    },
                    new UnitPrice
                    {
                        ArName = "كيس",
                        EnName = "Bag"
                    },
                    new UnitPrice
                    {
                        ArName = "قطعة",
                        EnName = "Piece"
                    },
                    new UnitPrice
                    {
                        ArName = "زجاجة",
                        EnName = "Bottle"
                    },
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
                    new ApplicationSetting
                    {
                        Password = ShiftPassword.Hash("@Qwe12"),
                        OrderTax = 5,
                        OrderDiscount = 1,
                        OrderService = 10,
                        PrintAfterOrder = false,
                        PrintForTakeawayOrder = false,
                        NumberOfCustomerPrint = 1,
                        NumberOfKitchenPrint = 1,
                        ShowTotalShift = true
                    }
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
                    new PrinterSetting
                    {
                        PrinterForAllSections = "Microsoft Print to PDF",
                        PrinterForCashier = "Microsoft Print to PDF",
                        PrinterForEachSection = "Microsoft Print to PDF"
                    }
                };
                context.PrinterSettings.AddRange(setting);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAdminRoleAndUsers(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
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
                new ApplicationUser
                {
                    FirstName = "ضيف الله",
                    LastName = "حمدان",
                    UserName = "Daif",
                    Email = "daif@gmail.com",
                    PhoneNumber = "01010164599",
                    UserType = "Admin",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    FirstName = "محمود",
                    LastName = "اسماعيل",
                    UserName = "Mahmoud",
                    Email = "mahmoud@gmail.com",
                    PhoneNumber = "01154809509",
                    UserType = "Admin",
                    EmailConfirmed = true
                }
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
