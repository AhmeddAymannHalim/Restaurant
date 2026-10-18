using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISupplierRepository
{
    Task<PaginatedResult<Supplier>> GetPagedSuppliersAsync(PageRequest request);
    Task<Supplier> GetSupplierByIdAsync(int id);
    Task<ResultMessage> UpsertSupplierAsync(Supplier entity);
    Task<int> CountInvoicesAsync(int id);
    Task<ResultMessage> DeleteSupplierAsync(int id);
}
