using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISupplierPaymentRepository
{
    Task<PaginatedResult<PaymentInvoiceSupplier>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentInvoiceSupplier> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<IEnumerable<LookupItem>> GetOpenInvoicesForPartnerAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceSupplier entity);
    Task<ResultMessage> DeletePaymentAsync(int id);

    Task<IEnumerable<PaymentRefund>> GetRefundsForPaymentAsync(int paymentId);
    Task<PaymentInvoiceSupplier> GetPaymentForRefundAsync(int id);
    Task<ResultMessage> RequestRefundAsync(PaymentRefund entity, string requestedByUserId);
    Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId);
    Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId);
}
