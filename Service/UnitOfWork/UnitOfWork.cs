using Restaurant.Data.Models;
using Service.Repositories;
using Service.IRepositories;
using Restaurant.Data;

namespace Service.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IGenericRepository<IndexSettings> IndexRepository { get ; set; }
        public IGenericRepository<ChefItem> ChefItemRepository { get ; set ; }
        public IGenericRepository<GalleryItem> GalleryItemRepository { get ; set ; }

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
        public IGenericRepository<MenuItem> MenuItemRepository { get ; set ; }
        public IGenericRepository<MenuItemComponent> MenuItemComponentRepository { get ; set ; }
        public IGenericRepository<Order> OrderRepository { get; set; }
        public IGenericRepository<OrderMenuItem> OrderMenuItemRepository { get; set; }
        public IGenericRepository<OnlineOrder> OnlineOrderRepository { get; set; }
        public IGenericRepository<OnlineOrderMenuItem> OnlineOrderMenuItemRepository { get; set; }
        public IGenericRepository<MenuSection> MenuSectionRepository { get; set; }
        public IGenericRepository<StoreConvert> StoreConvertRepository { get; set; }
        public IGenericRepository<StoreConvertProduct> StoreConvertProductRepository { get; set; }



        // By Yasser
        public IGenericRepository<Table> TableRepository { get; set; }
        public IGenericRepository<Customer> CustomerRepository { get; set; }
        public IGenericRepository<Expense> ExpenseRepository { get; set; }
        public IGenericRepository<State> StateRepository { get; set; }
        public IGenericRepository<Scrounge> ScroungeRepository { get; set; }
        public IGenericRepository<Deposit> DepositRepository { get; set; }


        public UnitOfWork(ApplicationDbContext context)
        {
            this._context = context;
         

            IndexRepository = new GenericRepository<IndexSettings>(_context);
            ChefItemRepository = new GenericRepository<ChefItem>(_context);
            GalleryItemRepository = new GenericRepository<GalleryItem>(_context);

            // Daif Hamdan
            BrancheRepository = new GenericRepository<Branche>(_context);
            JobRepository = new GenericRepository<Job>(_context);
            EmployeeRepository = new GenericRepository<Employee>(_context);
            SalaryTypeRepository = new GenericRepository<SalaryType>(_context);
            InvoiceRepository = new GenericRepository<Invoice>(_context);
            InvoiceItemRepository = new InvoiceItemRepository(_context);
            ProductRepository = new GenericRepository<Product>(_context);
            SupplierRepository = new GenericRepository<Supplier>(_context);
            UnitPriceRepository = new GenericRepository<UnitPrice>(_context);
            StoreRepository = new GenericRepository<Store>(_context);
            PaymentInvoiceCustomerRepository = new GenericRepository<PaymentInvoiceCustomer>(_context);
            PaymentInvoiceSupplierRepository = new GenericRepository<PaymentInvoiceSupplier>(_context);
            StoreProductRepository = new GenericRepository<StoreProduct>(_context);
            MenuItemRepository = new GenericRepository<MenuItem>(_context);
            MenuItemComponentRepository = new GenericRepository<MenuItemComponent>(_context);
            OrderRepository = new GenericRepository<Order>(_context);
            OrderMenuItemRepository = new GenericRepository<OrderMenuItem>(_context);
            OnlineOrderRepository = new GenericRepository<OnlineOrder>(_context);
            OnlineOrderMenuItemRepository = new GenericRepository<OnlineOrderMenuItem>(_context);
            MenuSectionRepository = new GenericRepository<MenuSection>(_context);
            StoreConvertRepository = new GenericRepository<StoreConvert>(_context);
            StoreConvertProductRepository = new GenericRepository<StoreConvertProduct>(_context);


            // Yasser
            TableRepository = new GenericRepository<Table>(_context);
            CustomerRepository = new GenericRepository<Customer>(_context);
            ExpenseRepository = new GenericRepository<Expense>(_context);
            StateRepository = new GenericRepository<State>(_context);
            ScroungeRepository = new GenericRepository<Scrounge>(_context);
            DepositRepository = new GenericRepository<Deposit>(_context);

        }

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
