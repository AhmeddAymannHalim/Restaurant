using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISupplierPaymentRepository
{
    Task<PaginatedResult<PaymentInvoiceSupplier>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentInvoiceSupplier> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceSupplier entity);
    Task<ResultMessage> DeletePaymentAsync(int id);
}
