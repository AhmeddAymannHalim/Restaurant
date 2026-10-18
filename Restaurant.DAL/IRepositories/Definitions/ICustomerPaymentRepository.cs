using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICustomerPaymentRepository
{
    Task<PaginatedResult<PaymentInvoiceCustomer>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentInvoiceCustomer> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<IEnumerable<LookupItem>> GetOpenInvoicesForPartnerAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceCustomer entity);
    Task<ResultMessage> DeletePaymentAsync(int id);

    Task<IEnumerable<PaymentRefund>> GetRefundsForPaymentAsync(int paymentId);
    Task<ResultMessage> RequestRefundAsync(PaymentRefund entity, string requestedByUserId);
    Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId);
    Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId);
}
