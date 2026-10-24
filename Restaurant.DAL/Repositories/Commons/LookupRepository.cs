using Restaurant.DAL.Repositories.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
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
    private readonly ApplicationDbContext _context;
    private readonly IStockRepository _stock;

    public LookupRepository(ApplicationDbContext context)
    {
        _context = context;
        _stock = new StockRepository(context);
    }



    public async Task<IEnumerable<SellTypeViewModel>> SellTypeLookupAsync()
    {
        return await _context.SellTypes.AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new SellTypeViewModel
            {
                Id = a.Id,
                Code = a.Code,
                ArName = a.ArName,
                EnName = a.EnName,
                Show = a.Show,
                IsLastPrice = a.IsLastPrice,
                PlusPrice = a.PlusPrice
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveTypeViewModel>> LeaveTypeLookupAsync()
    {
        return await _context.LeaveTypes.AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new LeaveTypeViewModel
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName,
                DaysPerYear = a.DaysPerYear
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<AllowanceTypeViewModel>> AllowanceTypeLookupAsync()
    {
        return await _context.AllowanceTypes.AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new AllowanceTypeViewModel
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName,
                Amount = a.Amount
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<DeductionTypeViewModel>> DeductionTypeLookupAsync()
    {
        return await _context.DeductionTypes.AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new DeductionTypeViewModel
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName,
                Amount = a.Amount
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<InsuranceCategoryViewModel>> InsuranceCategoryLookupAsync()
    {
        return await _context.InsuranceCategories.AsNoTracking()
            .OrderBy(a => a.ArName)
            .Select(a => new InsuranceCategoryViewModel
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName,
                BasicWage = a.BasicWage,
                AllowancesAmount = a.AllowancesAmount,
                EmployeePercent = a.EmployeePercent,
                CompanyPercent = a.CompanyPercent
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync()
    {
        return await _context.Employees
            .Where(a => a.Job.ArName == "طيار")
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        var storekeeperTitles = new List<string>();
        storekeeperTitles.Add("Store Keeper");
        storekeeperTitles.Add("Storekeeper");
        storekeeperTitles.Add("امين مخزن");
        storekeeperTitles.Add("امين مخازن");
        storekeeperTitles.Add("أمين مخازن");
        storekeeperTitles.Add("أمين مخزن");

        return await _context.Employees
            .Where(a => storekeeperTitles.Contains(a.Job.ArName) || storekeeperTitles.Contains(a.Job.EnName))
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> FreeTableLookupAsync()
    {
        return await _context.Tables.Where(a => !a.IsPending).Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName
        })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> SupplierLookupAsync()
    {
        return await _context.Suppliers.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.Account.ArName,
            EnName = a.Account.EnName
        })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> CustomerLookupAsync()
    {
        return await _context.Customers.Select(a => new LookupItem
        {
            Id = a.Id,
            ArName = a.Account.ArName,
            EnName = a.Account.EnName
        })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        var products = await _stock.ProductsOfStoreAsync(storeId);

        return products.OrderBy(a => LanguageHelper.IsArabic() ? a.ProductArName : a.ProductEnName).ToList();
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
    public async Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId)
    {
        return await _context.Governorates
            .Where(a => a.CountryId == countryId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId)
    {
        return await _context.Towns
            .Where(a => a.GovernorateId == governorateId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> NamedLookupAsync<T>() where T : class
    {
        return await _context.Set<T>().AsNoTracking()
            .OrderBy(a => EF.Property<string>(a, "ArName"))
            .Select(a => new LookupItem
            {
                Id = EF.Property<int>(a, "Id"),
                ArName = EF.Property<string>(a, "ArName"),
                EnName = EF.Property<string>(a, "EnName")
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
    public async Task<IEnumerable<LookupItem>> VillageLookupAsync(int townId)
    {
        return await _context.Villages
            .Where(a => a.TownId == townId)
            .Select(a => new LookupItem
            {
                Id = a.Id,
                ArName = a.ArName,
                EnName = a.EnName
            })
            .AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }
}
