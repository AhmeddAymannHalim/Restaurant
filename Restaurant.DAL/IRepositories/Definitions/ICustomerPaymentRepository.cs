using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICustomerPaymentRepository
{
    Task<PaginatedResult<PaymentInvoiceCustomer>> GetPagedPaymentsAsync(PageRequest request);
    Task<PaymentInvoiceCustomer> GetPaymentByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<decimal> GetBalanceAsync(int partnerId);
    Task<ResultMessage> UpsertPaymentAsync(PaymentInvoiceCustomer entity);
    Task<ResultMessage> DeletePaymentAsync(int id);
}
