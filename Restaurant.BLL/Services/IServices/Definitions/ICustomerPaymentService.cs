using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICustomerPaymentService
{
    Task<PaginatedResult<PaymentViewModel>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentViewModel> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<IEnumerable<LookupItem>> GetOpenInvoicesForPartnerAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentViewModel model);
    Task<ResultMessage> DeletePaymentAsync(int id);

    Task<PaymentRefundRequestViewModel> GetRefundRequestAsync(int paymentId);
    Task<ResultMessage> RequestRefundAsync(PaymentRefundRequestViewModel model, string requestedByUserId);
    Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId);
    Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId);
}
