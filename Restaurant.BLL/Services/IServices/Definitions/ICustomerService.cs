using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICustomerService
{
    Task<PaginatedResult<CustomerViewModel>> GetPagedCustomersAsync(PageRequest request);
    Task<CustomerViewModel> GetCustomerByIdAsync(int id);
    Task<ResultMessage> UpsertCustomerAsync(CustomerViewModel model);
    Task<ResultMessage> DeleteCustomerAsync(int id);
    Task<decimal?> GetCustomerBalanceAsync(int id);
}
