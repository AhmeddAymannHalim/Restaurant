using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICustomerTierRepository
{
    Task<PaginatedResult<CustomerTier>> GetPagedCustomerTiersAsync(PageRequest request);
    Task<CustomerTier> GetCustomerTierByIdAsync(int id);
    Task<ResultMessage> UpsertCustomerTierAsync(CustomerTier entity);
    Task<string> NewCustomerTierCodeAsync();
    Task<ResultMessage> DeleteCustomerTierAsync(int id);
}
