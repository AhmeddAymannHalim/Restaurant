namespace Shared.ViewModels.Definitions;

public class DashboardViewModel
{
    public string UserName { get; set; }
    public string UserImage { get; set; }

    // Operations
    public int PendingKitchenOrders { get; set; }
    public int Invoices { get; set; }
    public int StoreConverts { get; set; }
    public int SupplierPayments { get; set; }
    public int CustomerPayments { get; set; }

    // Organization
    public int Branches { get; set; }
    public int Jobs { get; set; }
    public int Employees { get; set; }
    public int EmployeeAccounts { get; set; }
    public int SalaryTypes { get; set; }
    public int Users { get; set; }
    public int Roles { get; set; }

    // Kitchen and menu
    public int Kitchens { get; set; }
    public int MenuSections { get; set; }
    public int MenuItems { get; set; }
    public int Tables { get; set; }

    // Inventory
    public int Stores { get; set; }
    public int Products { get; set; }
    public int UnitPrices { get; set; }

    // Partners
    public int Suppliers { get; set; }
    public int Customers { get; set; }

    // Finance
    public int Expenses { get; set; }
    public int ExitExpenses { get; set; }
    public int Deposits { get; set; }
    public int Scrounges { get; set; }
    public int Salaries { get; set; }

    public List<DashboardCustomerItem> TopCustomers { get; set; } = new();
}

public class DashboardCustomerItem
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string MobileNumber { get; set; }
    public decimal Balance { get; set; }
}
