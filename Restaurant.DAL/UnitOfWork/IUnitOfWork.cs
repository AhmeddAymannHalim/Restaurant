using Microsoft.AspNetCore.Identity;
using Restaurant.DAL.Models;
using Restaurant.DAL.IRepositories;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository<IndexSettings> IndexRepository { get; set; }
        public IGenericRepository<ChefItem> ChefItemRepository { get; set; }
        public IGenericRepository<GalleryItem> GalleryItemRepository { get; set; }


        // By Daif Hamdan
        public IGenericRepository<Branche> BrancheRepository { get; set; }
        public IGenericRepository<Job> JobRepository { get; set; }
        public IGenericRepository<Employee> EmployeeRepository { get; set; }
        public IGenericRepository<SalaryType> SalaryTypeRepository { get; set; }
        public IGenericRepository<Invoice> InvoiceRepository { get; set; }
        public IInvoiceItemRepository InvoiceItemRepository { get; set; }
        public IGenericRepository<Product> ProductRepository { get; set; }
        public IGenericRepository<Supplier> SupplierRepository { get; set; }
        public IGenericRepository<UnitPrice> UnitPriceRepository { get; set; }
        public IGenericRepository<Store> StoreRepository { get; set; }
        public IGenericRepository<PaymentInvoiceCustomer> PaymentInvoiceCustomerRepository { get; set; }
        public IGenericRepository<PaymentInvoiceSupplier> PaymentInvoiceSupplierRepository { get; set; }
        public IGenericRepository<StoreProduct> StoreProductRepository { get; set; }
        public IGenericRepository<MenuItem> MenuItemRepository { get; set; }
        public IGenericRepository<MenuItemComponent> MenuItemComponentRepository { get; set; }
        public IGenericRepository<Order> OrderRepository { get; set; }
        public IGenericRepository<OrderMenuItem> OrderMenuItemRepository { get; set; }
        public IGenericRepository<OnlineOrder> OnlineOrderRepository { get; set; }
        public IGenericRepository<OnlineOrderMenuItem> OnlineOrderMenuItemRepository { get; set; }
        public IGenericRepository<MenuSection> MenuSectionRepository { get; set; }
        public IGenericRepository<StoreConvert> StoreConvertRepository { get; set; }
        public IGenericRepository<StoreConvertProduct> StoreConvertProductRepository { get; set; }
        public IGenericRepository<ExitExpense> ExitExpenseRepository { get; set; }
        public IGenericRepository<Salary> SalaryRepository { get; set; }
        public IGenericRepository<ApplicationSetting> ApplicationSettingRepository { get; set; }
        public IGenericRepository<PrinterSetting> PrinterSettingRepository { get; set; }
        public IGenericRepository<PrinterForSection> PrinterForSectionRepository { get; set; }
        public IGenericRepository<ApplicationUser> UserRepository { get; set; }
        public IGenericRepository<IdentityRole> RoleRepository { get; set; }
        public IGenericRepository<Account> AccountRepository { get; set; }
        public IGenericRepository<AccountState> AccountStateRepository { get; set; }
        public IGenericRepository<AccountsType> AccountTypeRepository { get; set; }
        public IGenericRepository<Currency> CurrencyRepository { get; set; }
        public IGenericRepository<PayType> PayTypeRepository { get; set; }
        public IGenericRepository<SellType> SellTypeRepository { get; set; }




        // By Yasser
        public IGenericRepository<Table> TableRepository { get; set; }
        public IGenericRepository<Customer> CustomerRepository { get; set; }
        public IGenericRepository<Expense> ExpenseRepository { get; set; }
        public IGenericRepository<Scrounge> ScroungeRepository { get; set; }
        public IGenericRepository<Deposit> DepositRepository { get; set; }


        //By Mahmoud
        public IGenericRepository<kitchen> kitchenRepository { get; set; }

        // Definitions
        public IBranchRepository Branches { get; set; }
        public IJobRepository Jobs { get; set; }
        public ITableRepository Tables { get; set; }
        public IUnitPriceRepository UnitPrices { get; set; }
        public ISalaryTypeRepository SalaryTypes { get; set; }
        public IMenuSectionRepository MenuSections { get; set; }
        public IMenuItemRepository MenuItems { get; set; }
        public IProductRepository Products { get; set; }
        public IStoreRepository Stores { get; set; }
        public ISupplierRepository Suppliers { get; set; }
        public ICustomerRepository Customers { get; set; }

        public IKitchenRepository Kitchens { get; set; }
        public IExpenseRepository Expenses { get; set; }

        public IEmployeeRepository Employees { get; set; }

        public IDepositRepository Deposits { get; set; }
        public IExitExpenseRepository ExitExpenses { get; set; }
        public IScroungeRepository Scrounges { get; set; }
        public ISalaryRepository Salaries { get; set; }

        public IDashboardRepository Dashboard { get; set; }
        public ILookupRepository Lookups { get; set; }
        public IWebsiteRepository Website { get; set; }
        public IChefRepository Chefs { get; set; }
        public IGalleryRepository Galleries { get; set; }

        int Complete();

    }
}
