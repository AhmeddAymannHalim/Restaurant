using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISupplierPaymentService
{
    Task<PaginatedResult<PaymentViewModel>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentViewModel> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentViewModel model);
    Task<ResultMessage> DeletePaymentAsync(int id);
}
