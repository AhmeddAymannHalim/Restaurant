using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Commons;

public class LookupRepository : ILookupRepository
{
    private static readonly string[] StorekeeperTitles = { "Store Keeper", "امين مخزن", "امين مخازن", "أمين مخازن", "أمين مخزن" };

    private readonly ApplicationDbContext _context;

    public LookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LookupItem>> BranchLookupAsync()
    {
        return await ToListAsync(_context.Branches.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> JobLookupAsync()
    {
        return await ToListAsync(_context.Jobs.Select(a => new LookupItem
        {
            Id = a.JobId,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync()
    {
        return await ToListAsync(_context.SalaryTypes.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> EmployeeLookupAsync()
    {
        return await ToListAsync(_context.Employees.Select(a => new LookupItem
        {
            Id = a.EmployeeId,
            ArName = a.FirstName + " " + a.LastName,
            EnName = a.FirstName + " " + a.LastName
        }));
    }

    public async Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync()
    {
        return await ToListAsync(_context.Employees
            .Where(a => a.UserType == "طيار")
            .Select(a => new LookupItem
            {
                Id = a.EmployeeId,
                ArName = a.FirstName + " " + a.LastName,
                EnName = a.FirstName + " " + a.LastName
            }));
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await ToListAsync(_context.Employees
            .Where(a => StorekeeperTitles.Contains(a.UserType))
            .Select(a => new LookupItem
            {
                Id = a.EmployeeId,
                ArName = a.FirstName + " " + a.LastName,
                EnName = a.FirstName + " " + a.LastName
            }));
    }

    public async Task<IEnumerable<LookupItem>> KitchenLookupAsync()
    {
        return await ToListAsync(_context.Kitchens.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> MenuSectionLookupAsync()
    {
        return await ToListAsync(_context.MenuSections.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> ReadyMenuSectionLookupAsync()
    {
        return await ToListAsync(_context.MenuSections
            .Where(a => a.MenuItems.Any(i => i.ShowOnWebsite))
            .Select(a => new LookupItem
                {
                    Id = a.Id,
                    ArName = a.ArName,
                    EnName = a.EnName
                }));
    }

    public async Task<IEnumerable<LookupItem>> FreeTableLookupAsync()
    {
        return await ToListAsync(_context.Tables.Where(a => !a.IsPending).Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> TableLookupAsync()
    {
        return await ToListAsync(_context.Tables.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> StoreLookupAsync()
    {
        return await ToListAsync(_context.Stores.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> ProductLookupAsync()
    {
        return await ToListAsync(_context.Products.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> ProductCategoryLookupAsync()
    {
        return await ToListAsync(_context.ProductCategories.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> UnitPriceLookupAsync()
    {
        return await ToListAsync(_context.UnitPrices.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> SupplierLookupAsync()
    {
        return await ToListAsync(_context.Suppliers.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> CustomerLookupAsync()
    {
        return await ToListAsync(_context.Customers.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        var store = await _context.Stores
            .AsNoTracking()
            .Where(a => a.Id == storeId)
            .Select(a => new
            {
                a.ArName,
                a.EnName
            })
            .FirstOrDefaultAsync();

        var storeArName = store?.ArName;
        var storeEnName = store?.EnName;
        var isArabic = LanguageHelper.IsArabic();

        return await _context.Products
            .AsNoTracking()
            .Select(a => new StoreProductLookupItem
            {
                Id = a.StoreProducts
                    .Where(s => s.StoreId == storeId)
                    .Select(s => s.Id)
                    .FirstOrDefault(),
                ProductId = a.Id,
                ProductCategoryId = a.ProductCategoryId,
                StoreId = storeId,
                ProductArName = a.ArName,
                ProductEnName = a.EnName,
                StoreArName = storeArName,
                StoreEnName = storeEnName,
                Quantity = a.StoreProducts
                    .Where(s => s.StoreId == storeId)
                    .Select(s => s.Quantity)
                    .FirstOrDefault(),
                Cost = a.Cost
            })
            .OrderBy(a => isArabic ? a.ProductArName : a.ProductEnName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> ExpenseLookupAsync()
    {
        return await ToListAsync(_context.Expenses.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem_String>> RoleLookupAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new LookupItem_String
                {
                    Id = a.Name,
                    ArName = a.Name,
                    EnName = a.Name
                })
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> CountryLookupAsync()
    {
        return await ToListAsync(_context.Countries.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId)
    {
        return await ToListAsync(_context.Governorates
            .Where(a => a.CountryId == countryId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            }));
    }

    public async Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId)
    {
        return await ToListAsync(_context.Cities
            .Where(a => a.GovernorateId == governorateId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            }));
    }

    private static async Task<IEnumerable<LookupItem>> ToListAsync(IQueryable<LookupItem> query)
    {
        return await query.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
}
