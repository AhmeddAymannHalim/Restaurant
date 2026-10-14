using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierPaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<PaymentViewModel>> GetPagedPaymentsAsync(PageRequest request)
    {
        var result = await _unitOfWork.SupplierPayments.GetPagedPaymentsAsync(request);

        return PaginatedResult<PaymentViewModel>.Create(_mapper.Map<List<PaymentViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<PaymentViewModel> GetPaymentByIdAsync(int id)
    {
        var entity = await _unitOfWork.SupplierPayments.GetPaymentByIdAsync(id);

        return _mapper.Map<PaymentViewModel>(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _unitOfWork.SupplierPayments.GetNextCodeAsync();
    }

    public async Task<decimal> GetBalanceAsync(int partnerId)
    {
        return await _unitOfWork.SupplierPayments.GetBalanceAsync(partnerId);
    }

    public async Task<ResultMessage> UpsertPaymentAsync(PaymentViewModel model)
    {
        return await _unitOfWork.SupplierPayments.UpsertPaymentAsync(_mapper.Map<PaymentInvoiceSupplier>(model));
    }

    public async Task<ResultMessage> DeletePaymentAsync(int id)
    {
        return await _unitOfWork.SupplierPayments.DeletePaymentAsync(id);
    }

    public async Task<PaymentRefundRequestViewModel> GetRefundRequestAsync(int paymentId)
    {
        var payment = await _unitOfWork.SupplierPayments.GetPaymentByIdAsync(paymentId);
        if (payment is null)
        {
            return null;
        }

        var refunds = _mapper.Map<List<PaymentRefundViewModel>>(await _unitOfWork.SupplierPayments.GetRefundsForPaymentAsync(paymentId));
        var reserved = refunds.Where(a => a.Status != RefundStatus.Rejected).Sum(a => a.Amount);

        return new PaymentRefundRequestViewModel
        {
            OriginalPaymentId = payment.Id,
            OriginalPaymentCode = payment.ProcessCode,
            OriginalPaymentAmount = payment.Paid,
            RemainingRefundable = payment.Paid - reserved,
            Refunds = refunds
        };
    }

    public async Task<ResultMessage> RequestRefundAsync(PaymentRefundRequestViewModel model, string requestedByUserId)
    {
        var entity = new PaymentRefund
        {
            SupplierPaymentId = model.OriginalPaymentId,
            Amount = model.Amount,
            Reason = model.Reason
        };

        return await _unitOfWork.SupplierPayments.RequestRefundAsync(entity, requestedByUserId);
    }

    public async Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId)
    {
        return await _unitOfWork.SupplierPayments.ApproveRefundAsync(refundId, approvedByUserId);
    }

    public async Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId)
    {
        return await _unitOfWork.SupplierPayments.RejectRefundAsync(refundId, approvedByUserId);
    }
}
