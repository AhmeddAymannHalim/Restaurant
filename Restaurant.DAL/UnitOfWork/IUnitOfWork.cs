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
        public ICurrencyRepository Currencies { get; set; }
        public ICompanyRepository Companies { get; set; }
        public ILeaveTypeRepository LeaveTypes { get; set; }
        public IInsuranceCompanyRepository InsuranceCompanies { get; set; }
        public IInsuranceCategoryRepository InsuranceCategories { get; set; }
        public IJobTypeRepository JobTypes { get; set; }
        public IDepartmentRepository Departments { get; set; }
        public IContractTypeRepository ContractTypes { get; set; }
        public ISpecializationRepository Specializations { get; set; }
        public ICustomerTierRepository CustomerTiers { get; set; }
        public IClassificationRepository Classifications { get; set; }
        public ICostCenterRepository CostCenters { get; set; }
        public ISellTypeRepository SellTypes { get; set; }
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

        int Complete();
    }
}
