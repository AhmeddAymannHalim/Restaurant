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

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                     .Where(a => typeof(BaseEntity).IsAssignableFrom(a.ClrType))
                     .SelectMany(a => a.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }


    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobType> JobTypes { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<ContractType> ContractTypes { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Technician> Technicians { get; set; }
    public DbSet<InsuranceCategory> InsuranceCategories { get; set; }
    public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
    public DbSet<AllowanceType> AllowanceTypes { get; set; }
    public DbSet<DeductionType> DeductionTypes { get; set; }
    public DbSet<EmployeePhone> EmployeePhones { get; set; }
    public DbSet<CustomerPhone> CustomerPhones { get; set; }
    public DbSet<SupplierPhone> SupplierPhones { get; set; }
    public DbSet<StorePhone> StorePhones { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<EmployeeImage> EmployeeImages { get; set; }
    public DbSet<EmployeeDependent> EmployeeDependents { get; set; }
    public DbSet<EmployeeLeave> EmployeeLeaves { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<EmployeeAllowance> EmployeeAllowances { get; set; }
    public DbSet<EmployeeFixedDeduction> EmployeeFixedDeductions { get; set; }
    public DbSet<AttendanceInOut> AttendanceInOuts { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<EmployeeRequest> EmployeeRequests { get; set; }

    public DbSet<Kitchen> Kitchens { get; set; }
    public DbSet<DeliveryZone> DeliveryZones { get; set; }
    public DbSet<MenuSection> MenuSections { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuItemComponent> MenuItemComponents { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<TableReservation> TableReservations { get; set; }

    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<UnitPrice> UnitPrices { get; set; }
    public DbSet<StoreProductMovement> StoreProductMovements { get; set; }
    public DbSet<StoreConvert> StoreConverts { get; set; }
    public DbSet<StoreConvertProduct> StoreConvertProducts { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Governorate> Governorates { get; set; }
    public DbSet<Town> Towns { get; set; }
    public DbSet<Village> Villages { get; set; }
    public DbSet<PaymentInvoiceSupplier> PaymentInvoiceSupplier { get; set; }
    public DbSet<PaymentInvoiceCustomer> PaymentInvoiceCustomer { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountDetails> AccountDetails { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyBranch> CompanyBranches { get; set; }

    public DbSet<SellType> SellTypes { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<Classification> Classifications { get; set; }
    public DbSet<CustomerTier> CustomerTiers { get; set; }
    public DbSet<CustomerTierDiscount> CustomerTierDiscounts { get; set; }
    public DbSet<AccountSetting> AccountSettings { get; set; }
    public DbSet<Tax> Taxes { get; set; }

    public DbSet<BillSetting> BillSettings { get; set; }
    public DbSet<BillMaster> BillMasters { get; set; }
    public DbSet<BillDetail> BillDetails { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    public DbSet<Item> Items { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<ItemUnit> ItemUnits { get; set; }
    public DbSet<StoreItemUnit> StoreItemUnits { get; set; }
    public DbSet<ItemLocation> ItemLocations { get; set; }
    public DbSet<ItemBarcode> ItemBarcodes { get; set; }
    public DbSet<ItemDiscountWithQuantity> ItemDiscountWithQuantities { get; set; }
    public DbSet<ItemCostingError> ItemCostingErrors { get; set; }

    public DbSet<EntryMaster> EntryMasters { get; set; }
    public DbSet<EntryDetail> EntryDetails { get; set; }
    public DbSet<EntrySetting> EntrySettings { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderMenuItem> OrderMenuItems { get; set; }
    public DbSet<OrderIngredient> OrderIngredients { get; set; }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExitExpense> ExitExpenses { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<Scrounge> Scrounges { get; set; }

    public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
    public DbSet<PrinterSetting> PrinterSettings { get; set; }
}
