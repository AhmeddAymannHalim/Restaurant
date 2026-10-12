using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;

namespace Restaurant.DAL;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions) : base(contextOptions)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<IndexSettings> IndexSettings { get; set; }
    public DbSet<ChefItem> ChefItem { get; set; }
    public DbSet<GalleryItem> GalleryItem { get; set; }

    public DbSet<Branche> Branches { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<SalaryType> SalaryTypes { get; set; }
    public DbSet<Salary> Salaries { get; set; }

    public DbSet<Kitchen> Kitchens { get; set; }
    public DbSet<DeliveryZone> DeliveryZones { get; set; }
    public DbSet<MenuSection> MenuSections { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuItemComponent> MenuItemComponents { get; set; }
    public DbSet<Table> Tables { get; set; }

    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<UnitPrice> UnitPrices { get; set; }
    public DbSet<StoreProduct> StoreProducts { get; set; }
    public DbSet<StoreConvert> StoreConverts { get; set; }
    public DbSet<StoreConvertProduct> StoreConvertProducts { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<PaymentInvoiceSupplier> PaymentInvoiceSupplier { get; set; }
    public DbSet<PaymentInvoiceCustomer> PaymentInvoiceCustomer { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderMenuItem> OrderMenuItems { get; set; }
    public DbSet<OrderIngredient> OrderIngredients { get; set; }
    public DbSet<OnlineOrder> OnlineOrders { get; set; }
    public DbSet<OnlineOrderMenuItem> OnlineOrderMenuItems { get; set; }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExitExpense> ExitExpenses { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<Scrounge> Scrounges { get; set; }

    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<PrinterSetting> PrinterSettings { get; set; }
}
