using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Restaurant.DAL;
using Restaurant.DAL.IRepositories;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories;
using Restaurant.DAL.Repositories.Commons;
using Restaurant.DAL.Repositories.Definitions;

namespace Restaurant.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public UnitOfWork(ApplicationDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;

            Journals = new JournalEntryRepository(_context);

            Branches = new BranchRepository(_context, _httpContext);
            Jobs = new JobRepository(_context, _httpContext);
            UnitPrices = new UnitPriceRepository(_context, _httpContext);
            ProductCategories = new ProductCategoryRepository(_context, _httpContext);
            MenuSections = new MenuSectionRepository(_context, _httpContext);
            Products = new ProductRepository(_context, _httpContext);
            Kitchens = new KitchenRepository(_context, _httpContext);
            Currencies = new CurrencyRepository(_context, _httpContext);
            Companies = new CompanyRepository(_context, _httpContext);
            LeaveTypes = new LeaveTypeRepository(_context, _httpContext);
            InsuranceCompanies = new InsuranceCompanyRepository(_context, _httpContext);
            AllowanceTypes = new AllowanceTypeRepository(_context, _httpContext);
            DeductionTypes = new DeductionTypeRepository(_context, _httpContext);
            InsuranceCategories = new InsuranceCategoryRepository(_context, _httpContext);
            JobTypes = new JobTypeRepository(_context, _httpContext);
            Departments = new DepartmentRepository(_context, _httpContext);
            ContractTypes = new ContractTypeRepository(_context, _httpContext);
            Specializations = new SpecializationRepository(_context, _httpContext);
            CustomerTiers = new CustomerTierRepository(_context, _httpContext);
            Classifications = new ClassificationRepository(_context, _httpContext);
            CostCenters = new CostCenterRepository(_context, _httpContext);
            DeliveryZones = new DeliveryZoneRepository(_context, _httpContext);
            Expenses = new ExpenseRepository(_context, _httpContext);
            Tables = new TableRepository(_context, _httpContext);
            TableReservations = new TableReservationRepository(_context);
            MenuItems = new MenuItemRepository(_context, _httpContext);
            Stores = new StoreRepository(_context, _httpContext);
            Suppliers = new SupplierRepository(_context, _httpContext);
            Invoices = new InvoiceRepository(_context, Journals);
            StockMovements = new StockMovementRepository(_context);
            Orders = new OrderRepository(_context);
            Bills = new BillRepository(_context);
            AccountDefinitions = new AccountDefinitionRepository(_context);
            Entries = new EntryRepository(_context);
            Reports = new ReportRepository(_context);
            StoreConverts = new StoreConvertRepository(_context);
            SupplierPayments = new SupplierPaymentRepository(_context, Journals);
            CustomerPayments = new CustomerPaymentRepository(_context, Journals);
            Customers = new CustomerRepository(_context, _httpContext);
            Employees = new EmployeeRepository(_context, _httpContext);
            Dashboard = new DashboardRepository(_context);
            Deposits = new DepositRepository(_context, Journals);
            ExitExpenses = new ExitExpenseRepository(_context, Journals);
            Scrounges = new ScroungeRepository(_context, Journals);
            Salaries = new SalaryRepository(_context, Journals);
            Lookups = new LookupRepository(_context);
        }

        public IBranchRepository Branches { get; set; }
        public IJobRepository Jobs { get; set; }
        public ITableRepository Tables { get; set; }
        public ITableReservationRepository TableReservations { get; set; }
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
        public IBillRepository Bills { get; set; }
        public IAccountDefinitionRepository AccountDefinitions { get; set; }
        public IEntryRepository Entries { get; set; }
        public IReportRepository Reports { get; set; }
        public IStoreConvertRepository StoreConverts { get; set; }
        public ISupplierPaymentRepository SupplierPayments { get; set; }
        public ICustomerPaymentRepository CustomerPayments { get; set; }
        public ICustomerRepository Customers { get; set; }

        public IKitchenRepository Kitchens { get; set; }
        public ICurrencyRepository Currencies { get; set; }
        public ICompanyRepository Companies { get; set; }
        public ILeaveTypeRepository LeaveTypes { get; set; }
        public IInsuranceCompanyRepository InsuranceCompanies { get; set; }
        public IAllowanceTypeRepository AllowanceTypes { get; set; }
        public IDeductionTypeRepository DeductionTypes { get; set; }
        public IInsuranceCategoryRepository InsuranceCategories { get; set; }
        public IJobTypeRepository JobTypes { get; set; }
        public IDepartmentRepository Departments { get; set; }
        public IContractTypeRepository ContractTypes { get; set; }
        public ISpecializationRepository Specializations { get; set; }
        public ICustomerTierRepository CustomerTiers { get; set; }
        public IClassificationRepository Classifications { get; set; }
        public ICostCenterRepository CostCenters { get; set; }
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
