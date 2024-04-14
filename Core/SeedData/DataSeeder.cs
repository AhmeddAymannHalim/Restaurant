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

            // Account Guide
            await SeedCurrencies(context);
            await SeedPayTypes(context);
            await SeedAccountStates(context);
            await SeedAccountTypes(context);
            await SeedAccounts(context);
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


        // Account Guide
        private static async Task SeedCurrencies(ApplicationDbContext context)
        {
            if (!context.Currencies.Any())
            {
                var currencies = new[]
                {
                    new Currency {  ArName="الجنية المصرى", EnName="Pound"},
                    new Currency {  ArName="الدولار", EnName="Dollar"},
                };
                context.Currencies.AddRange(currencies);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPayTypes(ApplicationDbContext context)
        {
            if (!context.PayTypes.Any())
            {
                var types = new[]
                {
                    new PayType { PayTypeCode = "1", PayTypeArName="آجل", PayTypeEnName="Later", PayTypeNotes =null},
                    new PayType { PayTypeCode = "2", PayTypeArName="كاش", PayTypeEnName="Cash", PayTypeNotes =null},
                    new PayType { PayTypeCode = "3", PayTypeArName="فيزا", PayTypeEnName="Visa", PayTypeNotes =null},
                    new PayType { PayTypeCode = "4", PayTypeArName="نقدي", PayTypeEnName="Money", PayTypeNotes =null},
                };
                context.PayTypes.AddRange(types);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAccountTypes(ApplicationDbContext context)
        {
            if (!context.AccountTypes.Any())
            {
                var objects = new[]
                {
                    new AccountsType { AccTypeId = 1, AccTypeArName="عادي", AccTypeEnName="Normal"},
                    new AccountsType { AccTypeId = 2, AccTypeArName="ختامي", AccTypeEnName="Final"},
                    new AccountsType { AccTypeId = 3, AccTypeArName="تجميعي", AccTypeEnName="Collective"},
                    new AccountsType { AccTypeId = 4,  AccTypeArName="توزيعي", AccTypeEnName="Distributive"},
                    new AccountsType { AccTypeId = 5, AccTypeArName="عادي عميل", AccTypeEnName="Normal Cust"},
                    new AccountsType { AccTypeId = 6,  AccTypeArName="عادي مورد", AccTypeEnName="Normal Supp"},
                    new AccountsType { AccTypeId = 7,  AccTypeArName="عادي عميل و مورد", AccTypeEnName="Normal Cust And Supp"},
                    new AccountsType { AccTypeId = 8, AccTypeArName="عادي صندوق", AccTypeEnName="Normal Box"},
                    new AccountsType { AccTypeId = 9,  AccTypeArName="عادي بنك", AccTypeEnName="Normal Bank"},
                    new AccountsType { AccTypeId = 10, AccTypeArName="عادي صندوق و بنك", AccTypeEnName="Normal Box And Bank"},
                };
                context.AccountTypes.AddRange(objects);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAccountStates(ApplicationDbContext context)
        {
            if (!context.AccountStates.Any())
            {
                var states = new[]
                {
                    new AccountState { Id = 0, ArName="لا شئ", EnName="Nothing"},
                    new AccountState { Id = 1, ArName="مدين", EnName="Debit"},
                    new AccountState { Id = 2, ArName="دائن", EnName="Credit"},
                    new AccountState { Id = 3, ArName="اخري", EnName="Another"},
                };
                context.AccountStates.AddRange(states);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAccounts(ApplicationDbContext context)
        {
            if (!context.Accounts.Any())
            {
                var accounts = new[]
                {
                    new Account { AccCode = "9", AccArName = "الميزانية العمومية", AccEnName = "الميزانية العمومية", AccTypeId = 2, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 1, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "91", AccArName = "حـ/ الأرباح والخسائر", AccEnName = "حـ/ الأرباح والخسائر", AccTypeId = 2, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = 1, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "911", AccArName = "حـ/ المتاجرة", AccEnName = "حـ/ المتاجرة", AccTypeId = 2, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = 2, FinalAccId = null, AccLevel = 3, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1", AccArName = "الأصول", AccEnName = "الأصول", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = null, FinalAccId = 1, AccLevel = 1, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2", AccArName = "الخصوم", AccEnName = "الخصوم", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = null, FinalAccId = 1, AccLevel = 1, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "3", AccArName = "تكلفة المبيعات", AccEnName = "تكلفة المبيعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 2, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "4", AccArName = "ايرادات المبيعات", AccEnName = "ايرادات المبيعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 2, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5", AccArName = "المصروفات", AccEnName = "المصروفات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 3, FinalAccId = null, AccLevel = 3, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "6", AccArName = "الايرادات", AccEnName = "الايرادات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 3, FinalAccId = null, AccLevel = 3, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "11", AccArName = "الاصول الثابته", AccEnName = "الاصول الثابته", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 4, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "12", AccArName = "الاصول المتداولة", AccEnName = "الاصول المتداولة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 4, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "13", AccArName = "ارصدة مدينة اخري", AccEnName = "ارصدة مدينة اخري", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 4, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1101", AccArName = "صافي السيارات", AccEnName = "صافي السيارات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 10, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1102", AccArName = "صافي اجهزة الكمبيوتر", AccEnName = "اصافي اجهزة الكمبيوتر", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 10, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1103", AccArName = "صافي الاثاث والمفروشات", AccEnName = "صافي الاثاث والمفروشات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 10, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1201", AccArName = "نقدية بالصندوق", AccEnName = "نقدية بالصندوق", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 11, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1202", AccArName = "نقدية بالبنوك", AccEnName = "نقدية بالبنوك", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 11, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1203", AccArName = "ذمم العملاء", AccEnName = "ذمم العملاء", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 11, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1204", AccArName = "سلف الموظفين", AccEnName = "سلف الموظفين", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 11, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1301", AccArName = "مصاريف مقدمه", AccEnName = "مصاريف مقدمه", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 12, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1302", AccArName = "اوراق قبض بالحافظة", AccEnName = "اوراق قبض بالحافظة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 12, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "21", AccArName = "حقوق الملكية وراس المال", AccEnName = "حقوق الملكية وراس المال", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 5, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "22", AccArName = "الخصوم المتداولة", AccEnName = "الخصوم المتداولة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 5, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "23", AccArName = "ارصدة دائنة اخرى", AccEnName = "ارصدة دائنة اخرى", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 5, FinalAccId = 1, AccLevel = 2, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "211", AccArName = "راس المال", AccEnName = "راس المال", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 22, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "212", AccArName = "جاري الشركاء", AccEnName = "جاري الشركاء", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 22, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "213", AccArName = "الارباح", AccEnName = "الارباح", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 22, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "214", AccArName = "الاحتياطيات", AccEnName = "الاحتياطيات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 22, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2201", AccArName = "ذمم الموردين", AccEnName = "ذمم الموردين", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 23, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2301", AccArName = "مصروفات مستحقه", AccEnName = "مصروفات مستحقه", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 24, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2302", AccArName = "اوراق دفع بالحافظة", AccEnName = "اوراق دفع بالحافظة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 24, FinalAccId = 1, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "31", AccArName = "البضاعة", AccEnName = "البضاعة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 6, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "32", AccArName = "صافي المشتريات", AccEnName = "صافي المشتريات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 6, FinalAccId = 2, AccLevel = 3, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "311", AccArName = "بضاعة اول المدة", AccEnName = "بضاعة اول المدة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 32, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "312", AccArName = "بضاعة اخر المدة", AccEnName = "بضاعة اخر المدة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 32, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "321", AccArName = "المشتريات", AccEnName = "المشتريات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 33, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "322", AccArName = "مردودات المشتريات", AccEnName = "مردودات المشتريات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 33, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "323", AccArName = "مصاريف نقل المشتريات", AccEnName = "مصاريف نقل المشتريات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 33, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "324", AccArName = "الخصم المكتسب", AccEnName = "الخصم المكتسب", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 33, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "41", AccArName = "صافي المبيعات", AccEnName = "صافي المبيعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 7, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 2, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "411", AccArName = "المبيعات", AccEnName = "المبيعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 40, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "412", AccArName = "مردودات المبيعات", AccEnName = "مردودات المبيعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 40, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "413", AccArName = "الخصم المسموح به", AccEnName = "الخصم المسموح به", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 40, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "51", AccArName = "المصاريف الادارية والعمومية", AccEnName = "المصاريف الادارية والعمومية", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 8, FinalAccId = 2, AccLevel = 2, AccRemarks = null, AccNsons = 14, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5101", AccArName = "الرواتب", AccEnName = "الرواتب", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 44, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5102", AccArName = "الايجارات", AccEnName = "الايجارات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 44, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5103", AccArName = "عمولات بنكية", AccEnName = "عمولات بنكية", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 44, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5104", AccArName = "مصاريف دعاية واعلان", AccEnName = "مصاريف دعاية واعلان", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 44, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "5105", AccArName = "مصاريف نثرية", AccEnName = "مصاريف نثرية", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 44, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 3, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "61", AccArName = "ايرادات اخري ", AccEnName = "ايرادات اخري ", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 9, FinalAccId = 2, AccLevel = 2, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "611", AccArName = "ايرادات نشاط", AccEnName = "ايرادات نشاط", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 50, FinalAccId = 3, AccLevel = 3, AccRemarks = null, AccNsons = 1, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "12011", AccArName = "الصندوق الرئيسي", AccEnName = "الصندوق الرئيسي", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 16, FinalAccId = 4, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 3, AccCustomerCode = "1", Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1205", AccArName = "العهد", AccEnName = "العهد", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 3, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 11, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "12031", AccArName = "نقدا", AccEnName = "CASH", AccTypeId = 7, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 18, FinalAccId = 3, AccLevel = 1, AccRemarks = "1", AccNsons = 0, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = "1", Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1303", AccArName = "اوراق قبض تحت التحصيل", AccEnName = "اوراق قبض تحت التحصيل", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 12, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 1, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1304", AccArName = "اوراق قبض مرتدة", AccEnName = "اوراق قبض مرتدة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 12, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 1, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "1305", AccArName = "اوراق قبض تحصيل على دفعات", AccEnName = "اوراق قبض تحصيل على دفعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 2, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 12, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2303", AccArName = "اوراق دفع تحت السداد", AccEnName = "اوراق دفع تحت السداد", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 24, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 2, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2304", AccArName = "اوراق دفع مرتدة", AccEnName = "اوراق دفع مرتدة", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 24, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 2, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
                    new Account { AccCode = "2305", AccArName = "اوراق دفع سداد على دفعات", AccEnName = "اوراق دفع سداد على دفعات", AccTypeId = 1, AccDate = new DateTime(2020, 1, 1), AccCheckDate = new DateTime(2020, 1, 1), AccStateId = 1, AccCredit = 0, AccDebit = 0, AccMaxDebit = null, CurrencyId = 1, ParentAccId = 24, FinalAccId = 3, AccLevel = 1, AccRemarks = null, AccNsons = 0, AccTurnningYesOrNo = "1", AccDebitOrCreditOrWithout = 2, AccCustomerCode = null, Class = false, AccMustCostCenter = false },


                };
                context.Accounts.AddRange(accounts);
                await context.SaveChangesAsync();
            }
        }





        //private static async Task SeedAccounllts(ApplicationDbContext context)
        //{
        //    if (!context.Accounts.Any())
        //    {
        //        var accounts = new[]
        //        {
        //            new Account { AccId = 1, AccCode = "9", AccArName = "الميزانية العمومية", AccEnName = "الميزانية العمومية", AccTypeId = 2, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 1, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 2, AccCode = "8", AccArName = "الميزانية الرئيسية", AccEnName = "الميزانية الرئيسية", AccTypeId = 2, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 1, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 3, AccCode = "1", AccArName = "الأصول", AccEnName = "Assets", AccTypeId = 1, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 4, AccCode = "2", AccArName = "الإلتزامات", AccEnName = "Liabilities", AccTypeId = 1, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 5, AccCode = "3", AccArName = "حقوق الملكية", AccEnName = "Owner's Equity", AccTypeId = 1, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 6, AccCode = "4", AccArName = "الإيرادات", AccEnName = "Revenue", AccTypeId = 3, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 7, AccCode = "5", AccArName = "المصروفات", AccEnName = "Expenses", AccTypeId = 4, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 8, AccCode = "6", AccArName = "مركز التكلفة", AccEnName = "Cost Center", AccTypeId = 1, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 2, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //            new Account { AccId = 9, AccCode = "9", AccArName = "الأصول الثابتة", AccEnName = "Fixed Assets", AccTypeId = 1, AccDate = new DateTime(2024, 1, 1), AccCheckDate = new DateTime(2024, 1, 1), AccStateId = 0, AccCredit = 0, AccDebit = 0, AccMaxDebit = 0, CurrencyId = 1, ParentAccId = null, FinalAccId = null, AccLevel = 3, AccRemarks = null, AccNsons = 23, AccTurnningYesOrNo = "0", AccDebitOrCreditOrWithout = 3, AccCustomerCode = null, Class = false, AccMustCostCenter = false },
        //        };
        //        context.Accounts.AddRange(accounts);
        //        await context.SaveChangesAsync();
        //    }
        //}





    }
}
