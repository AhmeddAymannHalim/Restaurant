using Restaurant.DAL.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.DAL.SeedData
{
    public static class DataSeeder
    {
        public static async Task SeedCountries(ApplicationDbContext context)
        {
            if (!context.Countries.Any())
            {
                var rows = new[]
                {
                    new Country { ArName = "مصر", EnName = "Egypt", ArNationality = "مصري" , EnNationality = "Egyption"},
                    new Country { ArName = "السعودية", EnName = "Saudi Arabia", ArNationality = "سعودي" , EnNationality = "Saudi Arabian"},
                };
                context.Countries.AddRange(rows);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedGovs(ApplicationDbContext context)
        {
            if (!context.Governorates.Any())
            {
                var rows = new[]
                {
                    new Governorate {CountryId = 1, ArName = "القاهرة", EnName = "Cairo"},
                    new Governorate {CountryId = 1, ArName = "الجيزة", EnName = "Giza"},
                    new Governorate {CountryId = 1, ArName = "الإسكندرية", EnName = "Alexandria"},
                    new Governorate {CountryId = 1, ArName = "الدقهلية", EnName = "Dakahlia"},
                    new Governorate {CountryId = 1, ArName = "البحر الأحمر", EnName = "Red Sea"},
                    new Governorate {CountryId = 1, ArName = "البحيرة", EnName = "Beheira"},
                    new Governorate {CountryId = 1, ArName = "الفيوم", EnName = "Fayoum"},
                    new Governorate {CountryId = 1, ArName = "الغربية", EnName = "Gharbia"},
                    new Governorate {CountryId = 1, ArName = "الإسماعيلية", EnName = "Ismailia"},
                    new Governorate {CountryId = 1, ArName = "المنوفية", EnName = "Menofia"},
                    new Governorate {CountryId = 1, ArName = "المنيا", EnName = "Minya"},
                    new Governorate {CountryId = 1, ArName = "القليوبية", EnName = "Qalyubia"},
                    new Governorate {CountryId = 1, ArName = "الوادي الجديد", EnName = "New Valley"},
                    new Governorate {CountryId = 1, ArName = "السويس", EnName = "Suez"},
                    new Governorate {CountryId = 1, ArName = "أسوان", EnName = "Aswan"},
                    new Governorate {CountryId = 1, ArName = "أسيوط", EnName = "Asyut"},
                    new Governorate {CountryId = 1, ArName = "بني سويف", EnName = "Beni Suef"},
                    new Governorate {CountryId = 1, ArName = "بورسعيد", EnName = "Port Said"},
                    new Governorate {CountryId = 1, ArName = "دمياط", EnName = "Damietta"},
                    new Governorate {CountryId = 1, ArName = "الشرقية", EnName = "Sharkia"},
                    new Governorate {CountryId = 1, ArName = "جنوب سيناء", EnName = "South Sinai"},
                    new Governorate {CountryId = 1, ArName = "كفر الشيخ", EnName = "Kafr El Sheikh"},
                    new Governorate {CountryId = 1, ArName = "مطروح", EnName = "Matruh"},
                    new Governorate {CountryId = 1, ArName = "قنا", EnName = "Qena"},
                    new Governorate {CountryId = 1, ArName = "شمال سيناء", EnName = "North Sinai"},
                    new Governorate {CountryId = 1, ArName = "سوهاج", EnName = "Sohag"},
                    new Governorate {CountryId = 1, ArName = "الأقصر", EnName = "Luxor"},

                    new Governorate {CountryId = 2, ArName = "الرياض", EnName = "Riyadh"},
                    new Governorate {CountryId = 2, ArName = "مكة المكرمة", EnName = "Makkah"},
                    new Governorate {CountryId = 2, ArName = "جدة", EnName = "Jeddah"},
                    new Governorate {CountryId = 2, ArName = "المدينة المنورة", EnName = "Madinah"},
                    new Governorate {CountryId = 2, ArName = "الدمام", EnName = "Dammam"},
                    new Governorate {CountryId = 2, ArName = "الخبر", EnName = "Khobar"},
                    new Governorate {CountryId = 2, ArName = "أبها", EnName = "Abha"},
                    new Governorate {CountryId = 2, ArName = "تبوك", EnName = "Tabuk"},
                    new Governorate {CountryId = 2, ArName = "حائل", EnName = "Hail"},
                    new Governorate {CountryId = 2, ArName = "عرعر", EnName = "Arar"},
                    new Governorate {CountryId = 2, ArName = "جازان", EnName = "Jazan"},
                    new Governorate {CountryId = 2, ArName = "نجران", EnName = "Najran"},
                    new Governorate {CountryId = 2, ArName = "الباحة", EnName = "Al Bahah"},
                    new Governorate {CountryId = 2, ArName = "الجوف", EnName = "Al Jawf"},
                    new Governorate {CountryId = 2, ArName = "بريدة", EnName = "Buraidah"},
                    new Governorate {CountryId = 2, ArName = "سكاكا", EnName = "Sakaka"}
                };
                context.Governorates.AddRange(rows);
                await context.SaveChangesAsync();
            }

        }
        public static async Task SeedTowns(ApplicationDbContext context)
        {
            if (!context.Towns.Any())
            {
                var rows = new[]
                {
                    new Town {GovernorateId = 1, ArName = "القاهرة", EnName = "Cairo"},
                    new Town {GovernorateId = 1, ArName = "حلوان", EnName = "Helwan"},

                    new Town {GovernorateId = 2, ArName = "الجيزة", EnName = "Giza"},
                    new Town {GovernorateId = 2, ArName = "6 أكتوبر", EnName = "6th of October"},

                    new Town {GovernorateId = 3, ArName = "الإسكندرية", EnName = "Alexandria"},
                    new Town {GovernorateId = 3, ArName = "برج العرب", EnName = "Borg El Arab"},

                    new Town {GovernorateId = 4, ArName = "المنصورة", EnName = "Mansoura"},
                    new Town {GovernorateId = 4, ArName = "ميت غمر", EnName = "Mit Ghamr"},

                    new Town {GovernorateId = 5, ArName = "الغردقة", EnName = "Hurghada"},
                    new Town {GovernorateId = 5, ArName = "سفاجا", EnName = "Safaga"},

                    new Town {GovernorateId = 6, ArName = "دمنهور", EnName = "Damanhur"},
                    new Town {GovernorateId = 6, ArName = "كفر الدوار", EnName = "Kafr El Dawwar"},

                    new Town {GovernorateId = 7, ArName = "الفيوم", EnName = "Fayoum"},
                    new Town {GovernorateId = 7, ArName = "إطسا", EnName = "Itsa"},

                    new Town {GovernorateId = 8, ArName = "طنطا", EnName = "Tanta"},
                    new Town {GovernorateId = 8, ArName = "المحلة الكبرى", EnName = "El Mahalla El Kubra"},

                    new Town {GovernorateId = 9, ArName = "الإسماعيلية", EnName = "Ismailia"},
                    new Town {GovernorateId = 9, ArName = "فايد", EnName = "Fayed"},

                    new Town {GovernorateId = 10, ArName = "شبين الكوم", EnName = "Shibin El Kom"},
                    new Town {GovernorateId = 10, ArName = "منوف", EnName = "Menouf"},

                    new Town {GovernorateId = 11, ArName = "المنيا", EnName = "Minya"},
                    new Town {GovernorateId = 11, ArName = "مغاغة", EnName = "Maghagha"},

                    new Town {GovernorateId = 12, ArName = "بنها", EnName = "Banha"},
                    new Town {GovernorateId = 12, ArName = "قليوب", EnName = "Qalyub"},

                    new Town {GovernorateId = 13, ArName = "الخارجة", EnName = "Kharga"},
                    new Town {GovernorateId = 13, ArName = "باريس", EnName = "Paris"},

                    new Town {GovernorateId = 14, ArName = "السويس", EnName = "Suez"},
                    new Town {GovernorateId = 14, ArName = "عتاقة", EnName = "Ataqa"},

                    new Town {GovernorateId = 15, ArName = "أسوان", EnName = "Aswan"},
                    new Town {GovernorateId = 15, ArName = "دراو", EnName = "Draw"},

                    new Town {GovernorateId = 16, ArName = "أسيوط", EnName = "Asyut"},
                    new Town {GovernorateId = 16, ArName = "ديروط", EnName = "Dairut"},

                    new Town {GovernorateId = 17, ArName = "بني سويف", EnName = "Beni Suef"},
                    new Town {GovernorateId = 17, ArName = "الواسطى", EnName = "El Wasta"},

                    new Town {GovernorateId = 18, ArName = "بورسعيد", EnName = "Port Said"},
                    new Town {GovernorateId = 18, ArName = "العرب", EnName = "Al Arab"},

                    new Town {GovernorateId = 19, ArName = "دمياط", EnName = "Damietta"},
                    new Town {GovernorateId = 19, ArName = "رأس البر", EnName = "Ras El Bar"},

                    new Town {GovernorateId = 20, ArName = "الزقازيق", EnName = "Zagazig"},
                    new Town {GovernorateId = 20, ArName = "بلبيس", EnName = "Belbeis"},

                    new Town {GovernorateId = 21, ArName = "طور سيناء", EnName = "El Tor"},
                    new Town {GovernorateId = 21, ArName = "شرم الشيخ", EnName = "Sharm El Sheikh"},

                    new Town {GovernorateId = 22, ArName = "كفر الشيخ", EnName = "Kafr El Sheikh"},
                    new Town {GovernorateId = 22, ArName = "دسوق", EnName = "Desouk"},

                    new Town {GovernorateId = 23, ArName = "مرسى مطروح", EnName = "Marsa Matrouh"},
                    new Town {GovernorateId = 23, ArName = "الحمام", EnName = "El Hamam"},

                    new Town {GovernorateId = 24, ArName = "قنا", EnName = "Qena"},
                    new Town {GovernorateId = 24, ArName = "نجع حمادي", EnName = "Nag Hammadi"},

                    new Town {GovernorateId = 25, ArName = "العريش", EnName = "Arish"},
                    new Town {GovernorateId = 25, ArName = "رفح", EnName = "Rafah"},

                    new Town {GovernorateId = 26, ArName = "سوهاج", EnName = "Sohag"},
                    new Town {GovernorateId = 26, ArName = "طهطا", EnName = "Tahta"},

                    new Town {GovernorateId = 27, ArName = "الأقصر", EnName = "Luxor"},
                    new Town {GovernorateId = 27, ArName = "إسنا", EnName = "Esna"},

                    new Town {GovernorateId = 28, ArName = "الرياض", EnName = "Riyadh"},
                    new Town {GovernorateId = 28, ArName = "الدرعية", EnName = "Ad Diriyah"},

                    new Town {GovernorateId = 29, ArName = "مكة", EnName = "Mecca"},
                    new Town {GovernorateId = 29, ArName = "بحرة", EnName = "Bahrah"},

                    new Town {GovernorateId = 30, ArName = "جدة", EnName = "Jeddah"},
                    new Town {GovernorateId = 30, ArName = "ذهبان", EnName = "Dhaban"},

                    new Town {GovernorateId = 31, ArName = "المدينة المنورة", EnName = "Medina"},
                    new Town {GovernorateId = 31, ArName = "العلا", EnName = "Al Ula"},

                    new Town {GovernorateId = 32, ArName = "الدمام", EnName = "Dammam"},
                    new Town {GovernorateId = 32, ArName = "الجبيل", EnName = "Al Jubail"},

                    new Town {GovernorateId = 33, ArName = "الخبر", EnName = "Khobar"},
                    new Town {GovernorateId = 33, ArName = "الظهران", EnName = "Dhahran"},

                    new Town {GovernorateId = 34, ArName = "أبها", EnName = "Abha"},
                    new Town {GovernorateId = 34, ArName = "خميس مشيط", EnName = "Khamis Mushait"},

                    new Town {GovernorateId = 35, ArName = "تبوك", EnName = "Tabuk"},
                    new Town {GovernorateId = 35, ArName = "الوجه", EnName = "Al Wajh"},

                    new Town {GovernorateId = 36, ArName = "حائل", EnName = "Hail"},
                    new Town {GovernorateId = 36, ArName = "بقعاء", EnName = "Baq'a"},

                    new Town {GovernorateId = 37, ArName = "عرعر", EnName = "Arar"},
                    new Town {GovernorateId = 37, ArName = "رفحاء", EnName = "Rafha"},

                    new Town {GovernorateId = 38, ArName = "جازان", EnName = "Jazan"},
                    new Town {GovernorateId = 38, ArName = "أبو عريش", EnName = "Abu Arish"},

                    new Town {GovernorateId = 39, ArName = "نجران", EnName = "Najran"},
                    new Town {GovernorateId = 39, ArName = "شرورة", EnName = "Sharurah"},

                    new Town {GovernorateId = 40, ArName = "الباحة", EnName = "Al Bahah"},
                    new Town {GovernorateId = 40, ArName = "المندق", EnName = "Al Mandaq"},

                    new Town {GovernorateId = 41, ArName = "الجوف", EnName = "Al Jawf"},
                    new Town {GovernorateId = 41, ArName = "سكاكا", EnName = "Sakaka"},

                    new Town {GovernorateId = 42, ArName = "بريدة", EnName = "Buraidah"},
                    new Town {GovernorateId = 42, ArName = "عنيزة", EnName = "Unaizah"},

                    new Town {GovernorateId = 43, ArName = "دومة الجندل", EnName = "Dumat Al-Jandal"},
                    new Town {GovernorateId = 43, ArName = "القريات", EnName = "Al Qurayyat"}
                };
                context.Towns.AddRange(rows);
                await context.SaveChangesAsync();
            }

        }

        public static async Task SeedApplicationSettings(ApplicationDbContext context, string defaultPassword)
        {
            if (!context.ApplicationSettings.Any())
            {
                var setting = new[]
                {
                    new ApplicationSetting
                    {
                        Password = new ShiftPasswordRepository().Hash(defaultPassword ?? Guid.NewGuid().ToString("N")),
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

        public static async Task SeedPrinterSettings(ApplicationDbContext context)
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

        public static async Task SeedAdminRoleAndUsers(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            bool seedDefaultUsers,
            string defaultPassword)
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

            // sign-in by e-mail (manager@gmail.com) or by user name (المدير), password 0
            var existingManager = await userManager.FindByEmailAsync("manager@gmail.com");
            if (existingManager != null && existingManager.UserName != "المدير")
            {
                var renamed = await userManager.SetUserNameAsync(existingManager, "المدير");
                if (!renamed.Succeeded)
                {
                    Console.WriteLine("Manager rename failed: " + string.Join(", ", renamed.Errors.Select(e => e.Code)));
                }
            }
            if (existingManager == null)
            {
                var manager = new ApplicationUser
                {
                    FirstName = "المدير",
                    LastName = "",
                    UserName = "المدير",
                    Email = "manager@gmail.com",
                    UserType = "Admin",
                    EmailConfirmed = true
                };
                if ((await userManager.CreateAsync(manager, "0")).Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Admin");
                }
            }

            if (!seedDefaultUsers || string.IsNullOrEmpty(defaultPassword))
            {
                return;
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
                    FirstName = "احمد ايمن",
                    LastName = "حليم",
                    UserName = "Halim",
                    Email = "halim@gmail.com",
                    PhoneNumber = "01021487569",
                    UserType = "Admin",
                    EmailConfirmed = true
                }
            };

            foreach (var user in users)
            {
                var existingUser = await userManager.FindByNameAsync(user.UserName);
                if (existingUser == null)
                {
                    await userManager.CreateAsync(user, defaultPassword);
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
