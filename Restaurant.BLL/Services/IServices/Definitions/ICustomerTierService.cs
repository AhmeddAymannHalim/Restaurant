using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICustomerTierService
{
    Task<PaginatedResult<CustomerTierViewModel>> GetPagedCustomerTiersAsync(PageRequest request);
    Task<CustomerTierViewModel> GetCustomerTierByIdAsync(int id);
    Task<ResultMessage> UpsertCustomerTierAsync(CustomerTierViewModel model);
    Task<ResultMessage> DeleteCustomerTierAsync(int id);
    Task<string> NewCustomerTierCodeAsync();
}
