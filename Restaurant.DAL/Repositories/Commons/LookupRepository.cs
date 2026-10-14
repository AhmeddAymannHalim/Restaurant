using Restaurant.DAL.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Commons;

public class LookupRepository : ILookupRepository
{
    private static readonly string[] StorekeeperTitles = { "Store Keeper", "Storekeeper", "امين مخزن", "امين مخازن", "أمين مخازن", "أمين مخزن" };

    private readonly ApplicationDbContext _context;

    public LookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LookupItem>> BranchLookupAsync()
    {
        return await ToListAsync(_context.CompanyBranches.Select(a => new LookupItem
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
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync()
    {
        var items = Enum.GetValues<SalaryType>().Select(a => new LookupItem
        {
            Id = (int)a,
            ArName = a.ArName(),
            EnName = a.EnName()
        });

        return Task.FromResult(items);
    }

    public async Task<IEnumerable<LookupItem>> EmployeeLookupAsync()
    {
        return await ToListAsync(_context.Employees.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync()
    {
        return await ToListAsync(_context.Employees
            .Where(a => a.Job.ArName == "طيار")
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            }));
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await ToListAsync(_context.Employees
            .Where(a => StorekeeperTitles.Contains(a.Job.ArName) || StorekeeperTitles.Contains(a.Job.EnName))
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
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
            ArName = a.Account.ArName,
            EnName = a.Account.EnName
        }));
    }

    public async Task<IEnumerable<LookupItem>> CustomerLookupAsync()
    {
        return await ToListAsync(_context.Customers.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.Account.ArName,
            EnName = a.Account.EnName
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

        var quantities = await StoreStock.QuantitiesAsync(_context, storeId);
        var products = await _context.Products
            .AsNoTracking()
            .Select(a => new StoreProductLookupItem
            {
                Id = a.Id,
                ProductId = a.Id,
                ProductCategoryId = a.ProductCategoryId,
                StoreId = storeId,
                ProductArName = a.ArName,
                ProductEnName = a.EnName,
                StoreArName = storeArName,
                StoreEnName = storeEnName,
                Cost = a.Cost
            })
            .ToListAsync();

        foreach (var product in products)
        {
            product.Quantity = quantities.GetValueOrDefault((storeId, product.ProductId));
        }

        return products.OrderBy(a => isArabic ? a.ProductArName : a.ProductEnName).ToList();
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
        return await ToListAsync(_context.Towns
            .Where(a => a.GovernorateId == governorateId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            }));
    }

    public async Task<IEnumerable<LookupItem>> NamedLookupAsync<T>() where T : class
    {
        return await ToListAsync(_context.Set<T>().AsNoTracking()
            .OrderBy(a => EF.Property<string>(a, "ArName"))
            .Select(a => new LookupItem
            {
                Id = EF.Property<int>(a, "Id"),
                ArName = EF.Property<string>(a, "ArName"),
                EnName = EF.Property<string>(a, "EnName")
            }));
    }

    public async Task<IEnumerable<LookupItem>> VillageLookupAsync(int townId)
    {
        return await ToListAsync(_context.Villages
            .Where(a => a.TownId == townId)
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
