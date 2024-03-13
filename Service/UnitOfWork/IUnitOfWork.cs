using Restaurant.Data.Models;
using Service.IRepositories;

namespace Service.UnitOfWork
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




        // By Yasser
        public IGenericRepository<Table> TableRepository { get; set; }
        public IGenericRepository<Customer> CustomerRepository { get; set; }
        public IGenericRepository<Expense> ExpenseRepository { get; set; }
        public IGenericRepository<Scrounge> ScroungeRepository { get; set; }
        public IGenericRepository<Deposit> DepositRepository { get; set; }


        //By Mahmoud
        public IGenericRepository<kitchen> kitchenRepository { get; set; }


        int Complete();

    }
}
