using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Commons;

public class LookupRepository : ILookupRepository
{
    private readonly ApplicationDbContext _context;

    public LookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LookupItem>> BranchLookupAsync()
    {
        return await _context.Branches
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> JobLookupAsync()
    {
        return await _context.Jobs
            .Select(a => new LookupItem
            {
                Id = a.JobId,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync()
    {
        return await _context.SalaryTypes
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> KitchenLookupAsync()
    {
        return await _context.Kitchens
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> MenuSectionLookupAsync()
    {
        return await _context.MenuSections
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> StoreLookupAsync()
    {
        return await _context.Stores
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> ExpenseLookupAsync()
    {
        return await _context.Expenses
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> EmployeeLookupAsync()
    {
        return await _context.Employees
            .Select(a => new LookupItem
            {
                Id = a.EmployeeId,
                ArName = a.FirstName + " " + a.LastName,
                EnName = a.FirstName + " " + a.LastName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await _context.Employees
            .Where(a => a.UserType == "Store Keeper" || a.UserType == "امين مخزن" || a.UserType == "امين مخازن" || a.UserType == "أمين مخازن" || a.UserType == "أمين مخزن")
            .Select(a => new LookupItem
            {
                Id = a.EmployeeId,
                ArName = a.FirstName + " " + a.LastName,
                EnName = a.FirstName + " " + a.LastName
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        return await _context.StoreProducts
            .Where(a => a.StoreId == storeId)
            .OrderBy(a => a.ProductName)
            .Select(a => new StoreProductLookupItem
            {
                Id = a.Id,
                ProductId = a.ProductId,
                StoreId = a.StoreId,
                ProductName = a.ProductName,
                StoreName = a.StoreName,
                Quantity = a.Quantity,
                PriceForUnit = a.PriceForUnit
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<StoreProductLookupItem> StoreProductLookupByIdAsync(int id)
    {
        return await _context.StoreProducts
            .Where(a => a.Id == id)
            .Select(a => new StoreProductLookupItem
            {
                Id = a.Id,
                ProductId = a.ProductId,
                StoreId = a.StoreId,
                ProductName = a.ProductName,
                StoreName = a.StoreName,
                Quantity = a.Quantity,
                PriceForUnit = a.PriceForUnit
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
