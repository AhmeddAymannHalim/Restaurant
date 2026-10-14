using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IInvoiceRepository
{
    Task<PaginatedResult<Invoice>> GetPagedInvoicesAsync(PageRequest request);
    Task<Invoice> GetInvoiceByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetSupplierBalanceAsync(int supplierId);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task<ResultMessage> UpsertInvoiceAsync(Invoice entity);
    Task<ResultMessage> DeleteInvoiceAsync(int id);
}
