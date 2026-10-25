using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IInvoiceService
{
    Task<InvoiceViewModel> GetInvoiceByIdAsync(int id);
    Task<string> GetNextCodeAsync(InvoiceType type);
    Task<InvoiceViewModel> GetNewInvoiceAsync(InvoiceType type);
    Task<List<PostingDefaultViewModel>> GetPostingDefaultsAsync();
    Task<decimal> GetSupplierBalanceAsync(int supplierId);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task<int> GetCustomerSellTypeIdAsync(int customerId);
    Task<ResultMessage> UpsertInvoiceAsync(InvoiceViewModel model);
    Task<ResultMessage> DeleteInvoiceAsync(int id);
}
