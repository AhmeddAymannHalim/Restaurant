using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IInvoiceRepository
{
    Task<InvoiceViewModel> GetInvoiceByIdAsync(int id);
    Task<string> GetNextCodeAsync(InvoiceType type);
    Task<InvoiceViewModel> GetNewInvoiceAsync(InvoiceType type);
    Task<decimal> GetSupplierBalanceAsync(int supplierId);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task<int> GetCustomerSellTypeIdAsync(int customerId);
    Task<ResultMessage> UpsertInvoiceAsync(InvoiceViewModel model);
    Task<ResultMessage> DeleteInvoiceAsync(int id);
    Task<bool> IsInvoiceAsync(int id);
    Task<List<PostingDefaultViewModel>> GetPostingDefaultsAsync();
    Task<List<int>> GetUnpostedInvoiceIdsAsync();
    Task<ResultMessage> PostInvoiceAsync(int id);
}
