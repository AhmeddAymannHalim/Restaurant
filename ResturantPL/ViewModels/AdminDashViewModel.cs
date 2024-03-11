using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Restaurant.Data.Models;
using System;

namespace ResturantPL.ViewModels
{
    public class AdminDashViewModel
    {
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
        public ICollection<Branche> Branches { get; set; } = new List<Branche>();
        public ICollection<ChefItem> ChefItems { get; set; } = new List<ChefItem>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>(); // 
        public ICollection<Email> Emails { get; set; } = new List<Email>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();//
        public ICollection<ApplicationUser> EmployeeAccounts { get; set; } = new List<ApplicationUser>();//
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();//
        public ICollection<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
        public ICollection<IndexSettings> IndexSettings { get; set; } = new List<IndexSettings>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();//
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();//
        public ICollection<PaymentInvoiceCustomer> PaymentInvoiceCustomers { get; set; } = new List<PaymentInvoiceCustomer>();//
        public ICollection<PaymentInvoiceSupplier> PaymentInvoiceSuppliers { get; set; } = new List<PaymentInvoiceSupplier>();//
        public ICollection<Job> Jobs { get; set; } = new List<Job>();//
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<MenuSection> MenuSections { get; set; } = new List<MenuSection>();
        public ICollection<Product> Products { get; set; } = new List<Product>(); //
        public ICollection<SalaryType> SalaryTypes { get; set; } = new List<SalaryType>();
        public ICollection<Store> Stores { get; set; } = new List<Store>();//
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();//
        public ICollection<Table> Tables { get; set; } = new List<Table>();//
        public ICollection<UnitPrice> UnitPrice { get; set; } = new List<UnitPrice>();//
        public ICollection<StoreConvert> StoreConverts { get; set; } = new List<StoreConvert>();//
        public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();//
        public ICollection<kitchen> kitchens { get; set; } = new List<kitchen>();//
        public ICollection<Scrounge> Scrounges { get; set; } = new List<Scrounge>();//
        public ICollection<ExitExpense> ExitExpenses { get; set; } = new List<ExitExpense>();//
        public ICollection<Salary> Salaries { get; set; } = new List<Salary>();//


    }
}
