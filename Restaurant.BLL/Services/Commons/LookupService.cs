using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Commons;

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public LookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<LookupItem>> BranchLookupAsync()
    {
        return await _unitOfWork.Lookups.BranchLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> JobLookupAsync()
    {
        return await _unitOfWork.Lookups.JobLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync()
    {
        return await _unitOfWork.Lookups.SalaryTypeLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> EmployeeLookupAsync()
    {
        return await _unitOfWork.Lookups.EmployeeLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync()
    {
        return await _unitOfWork.Lookups.DeliveryDriverLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> FreeTableLookupAsync()
    {
        return await _unitOfWork.Lookups.FreeTableLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> StorekeeperLookupAsync()
    {
        return await _unitOfWork.Lookups.StorekeeperLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> KitchenLookupAsync()
    {
        return await _unitOfWork.Lookups.KitchenLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> MenuSectionLookupAsync()
    {
        return await _unitOfWork.Lookups.MenuSectionLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> ReadyMenuSectionLookupAsync()
    {
        return await _unitOfWork.Lookups.ReadyMenuSectionLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> TableLookupAsync()
    {
        return await _unitOfWork.Lookups.TableLookupAsync();
    }

    public async Task<IEnumerable<DeliveryZoneViewModel>> DeliveryZoneLookupAsync()
    {
        var zones = await _unitOfWork.DeliveryZones.GetAllDeliveryZonesAsync();

        return zones.Select(a => new DeliveryZoneViewModel
        {
            Id = a.Id,
            ArName = a.ArName,
            EnName = a.EnName,
            Fee = a.Fee
        }).ToList();
    }

    public async Task<IEnumerable<LookupItem>> StoreLookupAsync()
    {
        return await _unitOfWork.Lookups.StoreLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> ProductLookupAsync()
    {
        return await _unitOfWork.Lookups.ProductLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> ProductCategoryLookupAsync()
    {
        return await _unitOfWork.Lookups.ProductCategoryLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> UnitPriceLookupAsync()
    {
        return await _unitOfWork.Lookups.UnitPriceLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> SupplierLookupAsync()
    {
        return await _unitOfWork.Lookups.SupplierLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> CustomerLookupAsync()
    {
        return await _unitOfWork.Lookups.CustomerLookupAsync();
    }

    public async Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId)
    {
        return await _unitOfWork.Lookups.StoreProductLookupByStoreIdAsync(storeId);
    }

    public async Task<IEnumerable<LookupItem>> ExpenseLookupAsync()
    {
        return await _unitOfWork.Lookups.ExpenseLookupAsync();
    }

    public async Task<IEnumerable<LookupItem_String>> RoleLookupAsync()
    {
        return await _unitOfWork.Lookups.RoleLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> CountryLookupAsync()
    {
        return await _unitOfWork.Lookups.CountryLookupAsync();
    }

    public async Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId)
    {
        return await _unitOfWork.Lookups.GovernorateLookupAsync(countryId);
    }

    public async Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId)
    {
        return await _unitOfWork.Lookups.CityLookupAsync(governorateId);
    }
}
