using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IInvoiceService
{
    Task<PaginatedResult<InvoiceViewModel>> GetPagedInvoicesAsync(PageRequest request);
    Task<InvoiceViewModel> GetInvoiceByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetSupplierBalanceAsync(int supplierId);
    Task<ResultMessage> UpsertInvoiceAsync(InvoiceViewModel model);
    Task<ResultMessage> DeleteInvoiceAsync(int id);
}
