using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(a => a.Id == userId)
            .Select(a => new { a.FirstName, a.LastName, a.Image })
            .FirstOrDefaultAsync();

        // Only counts are loaded - never whole tables.
        var model = new DashboardViewModel
        {
            UserName = user == null ? string.Empty : (user.FirstName + " " + user.LastName).Trim(),
            UserImage = user?.Image,

            PendingKitchenOrders = await _context.Orders.AsNoTracking().CountAsync(a => !a.Complete),
            Invoices = await _context.Invoices.AsNoTracking().CountAsync(),
            StoreConverts = await _context.StoreConverts.AsNoTracking().CountAsync(),
            SupplierPayments = await _context.Set<PaymentInvoiceSupplier>().AsNoTracking().CountAsync(),
            CustomerPayments = await _context.Set<PaymentInvoiceCustomer>().AsNoTracking().CountAsync(),

            Branches = await _context.Branches.AsNoTracking().CountAsync(),
            Jobs = await _context.Jobs.AsNoTracking().CountAsync(),
            Employees = await _context.Employees.AsNoTracking().CountAsync(),
            EmployeeAccounts = await _context.Users.AsNoTracking().CountAsync(a => a.UserType != "User"),
            SalaryTypes = await _context.SalaryTypes.AsNoTracking().CountAsync(),
            Users = await _context.Users.AsNoTracking().CountAsync(),
            Roles = await _context.Roles.AsNoTracking().CountAsync(),

            Kitchens = await _context.Kitchens.AsNoTracking().CountAsync(),
            MenuSections = await _context.MenuSections.AsNoTracking().CountAsync(),
            MenuItems = await _context.MenuItems.AsNoTracking().CountAsync(),
            Tables = await _context.Tables.AsNoTracking().CountAsync(),

            Stores = await _context.Stores.AsNoTracking().CountAsync(),
            Products = await _context.Products.AsNoTracking().CountAsync(),
            UnitPrices = await _context.UnitPrices.AsNoTracking().CountAsync(),

            Suppliers = await _context.Suppliers.AsNoTracking().CountAsync(),
            Customers = await _context.Customers.AsNoTracking().CountAsync(),

            Expenses = await _context.Expenses.AsNoTracking().CountAsync(),
            ExitExpenses = await _context.ExitExpenses.AsNoTracking().CountAsync(),
            Deposits = await _context.Deposits.AsNoTracking().CountAsync(a => !a.IsDeleted),
            Scrounges = await _context.Scrounges.AsNoTracking().CountAsync(),
            Salaries = await _context.Salaries.AsNoTracking().CountAsync()
        };

        model.TopCustomers = await _context.Customers
            .AsNoTracking()
            .OrderByDescending(a => a.Balance)
            .Take(8)
            .Select(a => new DashboardCustomerItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName,
                MobileNumber = a.MobileNumber,
                Balance = a.Balance
            })
            .ToListAsync();

        return model;
    }
}
