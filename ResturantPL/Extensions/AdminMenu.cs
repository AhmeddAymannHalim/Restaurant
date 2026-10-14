namespace ResturantPL.Extensions;

public static class AdminMenu
{
    public record Item(string Controller, string Action, string Icon, string Label, bool ManagementOnly = false);

    public record Group(string Section, string Title, string Icon, Item[] Items);

    public static readonly Group[] Groups =
    {
        new("SectionOperations", "GroupOperations", "fa-cash-register", new Item[]
        {
            new("Order", "Upsert", "fa-cash-register", "NewOrder"),
            new("Order", "Index", "fa-gauge", "ShiftSummary"),
            new("Order", "List", "fa-bell-concierge", "Orders"),
            new("Order", "Online", "fa-globe", "OnlineOrders"),
            new("KitchenOrder", "Index", "fa-fire-burner", "KitchenOrders"),
            new("Invoice", "Index", "fa-file-invoice", "Invoices"),
            new("StoreConvert", "Index", "fa-right-left", "StoreConverts"),
            new("StockMovement", "Index", "fa-arrow-right-arrow-left", "StockMovements"),
            new("SupplierPayment", "Index", "fa-file-invoice-dollar", "SupplierPayments"),
            new("CustomerPayment", "Index", "fa-hand-holding-dollar", "CustomerPayments")
        }),
        new("SectionOperations", "GroupReports", "fa-chart-column", new Item[]
        {
            new("Report", "Index", "fa-chart-column", "Reports", true)
        }),
        new("SectionDefinitions", "GroupOrganization", "fa-sitemap", new Item[]
        {
            new("Branch", "Index", "fa-code-branch", "Branches"),
            new("Job", "Index", "fa-id-badge", "Jobs"),
            new("Employee", "Index", "fa-users", "Employees"),
            new("EmployeeAccount", "Index", "fa-user-lock", "EmployeeAccounts", true),
            new("User", "Index", "fa-user-group", "Users", true),
            new("Role", "Index", "fa-user-shield", "Roles", true),
            new("Specialization", "Index", "fa-user-graduate", "Specializations"),
            new("ContractType", "Index", "fa-file-signature", "ContractTypes"),
            new("Department", "Index", "fa-building", "Departments"),
            new("JobType", "Index", "fa-briefcase", "JobTypes"),
            new("InsuranceCategory", "Index", "fa-shield-heart", "InsuranceCategories"),
            new("InsuranceCompany", "Index", "fa-building-shield", "InsuranceCompanies"),
            new("LeaveType", "Index", "fa-umbrella-beach", "LeaveTypes")
        }),
        new("SectionDefinitions", "GroupKitchen", "fa-utensils", new Item[]
        {
            new("Kitchen", "Index", "fa-kitchen-set", "Kitchens"),
            new("MenuSection", "Index", "fa-list", "MenuSections"),
            new("Menu", "Index", "fa-utensils", "MenuItems"),
            new("Table", "Index", "fa-chair", "Tables"),
            new("DeliveryZone", "Index", "fa-motorcycle", "DeliveryZones")
        }),
        new("SectionDefinitions", "GroupInventory", "fa-boxes-stacked", new Item[]
        {
            new("Store", "Index", "fa-warehouse", "Stores"),
            new("Product", "Index", "fa-box", "Products"),
            new("ProductCategory", "Index", "fa-tags", "ProductCategories"),
            new("UnitPrice", "Index", "fa-weight-scale", "UnitPrices")
        }),
        new("SectionDefinitions", "GroupPartners", "fa-handshake", new Item[]
        {
            new("Supplier", "Index", "fa-truck", "Suppliers"),
            new("Customer", "Index", "fa-user-tag", "Customers"),
            new("SellType", "Index", "fa-tags", "SellTypes"),
            new("Classification", "Index", "fa-layer-group", "Classifications"),
            new("CustomerTier", "Index", "fa-medal", "CustomerTiers")
        }),
        new("SectionDefinitions", "GroupFinance", "fa-coins", new Item[]
        {
            new("Expense", "Index", "fa-receipt", "Expenses"),
            new("ExitExpense", "Index", "fa-money-bill-transfer", "ExitExpenses"),
            new("Deposit", "Index", "fa-piggy-bank", "Deposits"),
            new("Scrounge", "Index", "fa-hand-holding-dollar", "Scrounges"),
            new("Salary", "Index", "fa-wallet", "Salaries"),
            new("CostCenter", "Index", "fa-bullseye", "CostCenters"),
            new("Company", "Index", "fa-city", "Companies"),
            new("Currency", "Index", "fa-money-bill", "Currencies")
        }),
        new("SectionDefinitions", "Website", "fa-globe", new Item[]
        {
            new("Chef", "Index", "fa-user-tie", "Chefs"),
            new("Gallery", "Index", "fa-images", "Gallery")
        }),
    };

    public static Item Find(string controller)
    {
        return Groups.SelectMany(g => g.Items).FirstOrDefault(i => string.Equals(i.Controller,
            controller,
            StringComparison.OrdinalIgnoreCase));
    }
}
