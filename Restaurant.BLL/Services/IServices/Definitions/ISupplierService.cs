using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISupplierService
{
    Task<PaginatedResult<SupplierViewModel>> GetPagedSuppliersAsync(PageRequest request);
    Task<SupplierViewModel> GetSupplierByIdAsync(int id);
    Task<ResultMessage> UpsertSupplierAsync(SupplierViewModel model);
    Task<ResultMessage> DeleteSupplierAsync(int id);
}
