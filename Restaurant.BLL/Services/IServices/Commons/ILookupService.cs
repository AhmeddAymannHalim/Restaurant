using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Commons;

public interface ILookupService
{
    Task<IEnumerable<LookupItem>> BranchLookupAsync();
    Task<IEnumerable<LookupItem>> JobLookupAsync();
    Task<IEnumerable<LeaveTypeViewModel>> LeaveTypeLookupAsync();
    Task<IEnumerable<AllowanceTypeViewModel>> AllowanceTypeLookupAsync();
    Task<IEnumerable<DeductionTypeViewModel>> DeductionTypeLookupAsync();
    Task<IEnumerable<InsuranceCategoryViewModel>> InsuranceCategoryLookupAsync();
    Task<IEnumerable<LookupItem>> EmployeeLookupAsync();
    Task<IEnumerable<LookupItem>> StorekeeperLookupAsync();
    Task<IEnumerable<LookupItem>> FreeTableLookupAsync();
    Task<IEnumerable<LookupItem>> DeliveryDriverLookupAsync();
    Task<IEnumerable<LookupItem>> KitchenLookupAsync();
    Task<IEnumerable<LookupItem>> MenuSectionLookupAsync();
    Task<IEnumerable<LookupItem>> ReadyMenuSectionLookupAsync();
    Task<IEnumerable<LookupItem>> TableLookupAsync();
    Task<IEnumerable<LookupItem>> StoreLookupAsync();
    Task<IEnumerable<DeliveryZoneViewModel>> DeliveryZoneLookupAsync();
    Task<IEnumerable<LookupItem>> ProductLookupAsync();
    Task<IEnumerable<LookupItem>> UnitPriceLookupAsync();
    Task<IEnumerable<LookupItem>> ProductCategoryLookupAsync();
    Task<IEnumerable<LookupItem>> SupplierLookupAsync();
    Task<IEnumerable<LookupItem>> CustomerLookupAsync();
    Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId);
    Task<IEnumerable<LookupItem>> ExpenseLookupAsync();
    Task<IEnumerable<LookupItem_String>> RoleLookupAsync();

    Task<IEnumerable<LookupItem>> CountryLookupAsync();
    Task<IEnumerable<LookupItem>> GovernorateLookupAsync(int countryId);
    Task<IEnumerable<LookupItem>> CityLookupAsync(int governorateId);
    Task<IEnumerable<LookupItem>> NamedLookupAsync<T>() where T : class;
    Task<IEnumerable<LookupItem>> VillageLookupAsync(int townId);
}
