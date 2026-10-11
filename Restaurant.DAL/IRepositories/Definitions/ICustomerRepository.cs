using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICustomerRepository
{
    Task<PaginatedResult<Customer>> GetPagedCustomersAsync(PageRequest request);
    Task<Customer> GetCustomerByIdAsync(int id);
    Task<ResultMessage> UpsertCustomerAsync(Customer entity);
    Task<ResultMessage> DeleteCustomerAsync(int id);
    Task<decimal?> GetCustomerBalanceAsync(int id);
}
