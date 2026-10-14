using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories;
using Restaurant.DAL.IRepositories;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Repositories.Definitions;
using Restaurant.DAL;
using Microsoft.AspNetCore.Identity;

namespace Restaurant.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            this._context = context;

            Journals = new JournalEntryRepository(_context);

            Branches = new BranchRepository(_context);
            Jobs = new JobRepository(_context);
            UnitPrices = new UnitPriceRepository(_context);
            ProductCategories = new ProductCategoryRepository(_context);
            MenuSections = new MenuSectionRepository(_context);
            Products = new ProductRepository(_context);
            Kitchens = new KitchenRepository(_context);
            DeliveryZones = new DeliveryZoneRepository(_context);
            Expenses = new ExpenseRepository(_context);
            Tables = new TableRepository(_context);
            MenuItems = new MenuItemRepository(_context);
            Stores = new StoreRepository(_context);
            Suppliers = new SupplierRepository(_context);
            Invoices = new InvoiceRepository(_context, Journals);
            StockMovements = new StockMovementRepository(_context);
            Orders = new OrderRepository(_context);
            OnlineOrders = new OnlineOrderRepository(_context);
            Reports = new ReportRepository(_context);
            StoreConverts = new StoreConvertRepository(_context);
            SupplierPayments = new SupplierPaymentRepository(_context, Journals);
            CustomerPayments = new CustomerPaymentRepository(_context, Journals);
            Customers = new CustomerRepository(_context);
            Employees = new EmployeeRepository(_context);
            Dashboard = new DashboardRepository(_context);
            Deposits = new DepositRepository(_context);
            ExitExpenses = new ExitExpenseRepository(_context);
            Scrounges = new ScroungeRepository(_context);
            Salaries = new SalaryRepository(_context);
            Lookups = new LookupRepository(_context);
            Home = new HomeRepository(_context);
            Chefs = new ChefRepository(_context);
            Galleries = new GalleryRepository(_context);
        }

        public IBranchRepository Branches { get; set; }
        public IJobRepository Jobs { get; set; }
        public ITableRepository Tables { get; set; }
        public IUnitPriceRepository UnitPrices { get; set; }
        public IProductCategoryRepository ProductCategories { get; set; }
        public IMenuSectionRepository MenuSections { get; set; }
        public IMenuItemRepository MenuItems { get; set; }
        public IProductRepository Products { get; set; }
        public IStoreRepository Stores { get; set; }
        public ISupplierRepository Suppliers { get; set; }
        public IInvoiceRepository Invoices { get; set; }
        public IStockMovementRepository StockMovements { get; set; }
        public IOrderRepository Orders { get; set; }
        public IOnlineOrderRepository OnlineOrders { get; set; }
        public IReportRepository Reports { get; set; }
        public IStoreConvertRepository StoreConverts { get; set; }
        public ISupplierPaymentRepository SupplierPayments { get; set; }
        public ICustomerPaymentRepository CustomerPayments { get; set; }
        public ICustomerRepository Customers { get; set; }

        public IKitchenRepository Kitchens { get; set; }
        public IDeliveryZoneRepository DeliveryZones { get; set; }
        public IExpenseRepository Expenses { get; set; }

        public IEmployeeRepository Employees { get; set; }

        public IDepositRepository Deposits { get; set; }
        public IExitExpenseRepository ExitExpenses { get; set; }
        public IScroungeRepository Scrounges { get; set; }
        public ISalaryRepository Salaries { get; set; }

        public IDashboardRepository Dashboard { get; set; }
        public ILookupRepository Lookups { get; set; }
        public IJournalEntryRepository Journals { get; set; }
        public IHomeRepository Home { get; set; }
        public IChefRepository Chefs { get; set; }
        public IGalleryRepository Galleries { get; set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
