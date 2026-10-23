using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Commons;

public interface ILookupRepository
{
    Task<IEnumerable<LeaveTypeViewModel>> LeaveTypeLookupAsync();
    Task<IEnumerable<SellTypeViewModel>> SellTypeLookupAsync();
    Task<IEnumerable<AllowanceTypeViewModel>> AllowanceTypeLookupAsync();
    Task<IEnumerable<DeductionTypeViewModel>> DeductionTypeLookupAsync();
    Task<IEnumerable<InsuranceCategoryViewModel>> InsuranceCategoryLookupAsync();
    Task<IEnumerable<LookupItem>> StorekeeperLookupAsync();
    Task<IEnumerable<LookupItem>> FreeTableLookupAsync();
    Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync();


    Task<IEnumerable<LookupItem>> SupplierLookupAsync();
    Task<IEnumerable<LookupItem>> CustomerLookupAsync();
    Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId);


    Task<IEnumerable<LookupItem_String>> RoleLookupAsync();

    Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId);
    Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId);
    Task<IEnumerable<LookupItem>> NamedLookupAsync<T>() where T : class;
    Task<IEnumerable<LookupItem>> VillageLookupAsync(int townId);
}
