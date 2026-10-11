using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Commons;

public interface ILookupService
{
    Task<IEnumerable<LookupItem>> BranchLookupAsync();
    Task<IEnumerable<LookupItem>> JobLookupAsync();
    Task<IEnumerable<LookupItem>> SalaryTypeLookupAsync();
    Task<IEnumerable<LookupItem>> KitchenLookupAsync();
    Task<IEnumerable<LookupItem>> MenuSectionLookupAsync();
    Task<IEnumerable<LookupItem>> StoreLookupAsync();
    Task<IEnumerable<LookupItem>> ExpenseLookupAsync();
    Task<IEnumerable<LookupItem>> EmployeeLookupAsync();
    Task<IEnumerable<LookupItem>> StorekeeperLookupAsync();
    Task<IEnumerable<StoreProductLookupItem>> StoreProductLookupByStoreIdAsync(int storeId);
    Task<StoreProductLookupItem> StoreProductLookupByIdAsync(int id);
}
