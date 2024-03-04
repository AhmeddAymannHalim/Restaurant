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
        public static void SeedData(ApplicationDbContext context)
        {

            if (!context.Jobs.Any())
            {
                var jobs = new[]
                {
                new Job {  Name = "كاشير", Code = "7456" },
                new Job {  Name = "طيار", Code = "1425" },
                new Job {  Name = "امين مخزن", Code = "5241" },
                new Job {  Name = "جرسون", Code = "9841" },
                new Job {  Name = "طباخ", Code = "3452" },
                new Job {  Name = "نادل الصالة", Code = "4245" },
            };
                context.Jobs.AddRange(jobs);
                context.SaveChanges();
            }


            if (!context.IndexSettings.Any())
            {
                var indexes = new[]
                {
                new IndexSettings {  Name = "الاكلات الشهية", Description = "جميع الاطعمة و المشويات", Email = "di.hamdan55@gmail.com", StartDay = "السبت", EndDay = "السبت", Phone=  "01010164599", Locaion = "القاهرة رمسيس شارع الفجالة", StartHour = "9", EndHour = "12", ImageUrl = null,  VedioUrl= null }
            };
                context.IndexSettings.AddRange(indexes);
                context.SaveChanges();
            }


            if (!context.Tables.Any())
            {
                var tables = new[]
                {
                new Table {  Name = "طاولة رقم 1 ", IsPending = false, NumOfPersons = "4"},
                new Table {  Name = "طاولة رقم 2 ", IsPending = false, NumOfPersons = "6"},
                new Table {  Name = "طاولة رقم 3 ", IsPending = false, NumOfPersons = "8"},
                new Table {  Name = "طاولة رقم 4 ", IsPending = false, NumOfPersons = "3"},
                new Table {  Name = "طاولة رقم 5 ", IsPending = false, NumOfPersons = "2"},
                new Table {  Name = "طاولة رقم 6 ", IsPending = false, NumOfPersons = "4"},
                new Table {  Name = "طاولة رقم 7 ", IsPending = false, NumOfPersons = "5"},
            };
                context.Tables.AddRange(tables);
                context.SaveChanges();
            }



            if (!context.SalaryTypes.Any())
            {
                var types = new[]
                {
                new SalaryType {  Type = "سنوي"},
                new SalaryType {  Type = "شهري"},
                new SalaryType {  Type = "اسبوعي"},
                new SalaryType {  Type = "يومي"},
                new SalaryType {  Type = "تدريب مجاني"},
            };
                context.SalaryTypes.AddRange(types);
                context.SaveChanges();
            }

            if (!context.UnitPrices.Any())
            {
                var units = new[]
                {
                new UnitPrice {  Name = "كيلو"},
                new UnitPrice {  Name = "جرام"},
                new UnitPrice {  Name = "طن"},
                new UnitPrice {  Name = "علبة"},
                new UnitPrice {  Name = "لتر"},
                new UnitPrice {  Name = "كيس"},
                new UnitPrice {  Name = "قطعة"},
                new UnitPrice {  Name = "زجاجة"},
            };
                context.UnitPrices.AddRange(units);
                context.SaveChanges();
            }





            //if (!context.Users.Any())
            //{
            //    var users = new[]
            //    {
            //    new ApplicationUser {  FirstName = "Daif", LastName="Hamdan", MobileNumber= "01154809509", PhoneNumber = "01010164599", UserName= "Daif",Email = "daif@gmail.com", UserType = "User" },
            //    new ApplicationUser {  FirstName = "Mahmoud", LastName="Ismail", MobileNumber= "000000000000", PhoneNumber = "00000000000", UserName= "Mahmoud",Email = "mahmoud@gmail.com", UserType = "User" },
            //    new ApplicationUser {  FirstName = "Yasser", LastName="Gad", MobileNumber= "01154809509", PhoneNumber = "01010164599", UserName= "Yasser",Email = "yasser@gmail.com", UserType = "User" }
            //};
            //    context.Users.AddRange(users);
            //    context.SaveChanges();
            //}
        }
    }


}
