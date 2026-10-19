using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CustomerPaymentService : ICustomerPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerPaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<PaymentViewModel>> GetPagedPaymentsAsync(PageRequest request)
    {
        var result = await _unitOfWork.CustomerPayments.GetPagedPaymentsAsync(request);

        return PaginatedResult<PaymentViewModel>.Create(_mapper.Map<List<PaymentViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<PaymentViewModel> GetPaymentByIdAsync(int id)
    {
        var entity = await _unitOfWork.CustomerPayments.GetPaymentByIdAsync(id);

        return _mapper.Map<PaymentViewModel>(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _unitOfWork.CustomerPayments.GetNextCodeAsync();
    }

    public async Task<decimal> GetBalanceAsync(int partnerId)
    {
        return await _unitOfWork.CustomerPayments.GetBalanceAsync(partnerId);
    }

    public async Task<IEnumerable<LookupItem>> GetOpenInvoicesForPartnerAsync(int partnerId)
    {
        return await _unitOfWork.CustomerPayments.GetOpenInvoicesForPartnerAsync(partnerId);
    }

    public async Task<ResultMessage> UpsertPaymentAsync(PaymentViewModel model)
    {
        return await _unitOfWork.CustomerPayments.UpsertPaymentAsync(_mapper.Map<PaymentInvoiceCustomer>(model));
    }

    public async Task<ResultMessage> DeletePaymentAsync(int id)
    {
        return await _unitOfWork.CustomerPayments.DeletePaymentAsync(id);
    }

    public async Task<PaymentRefundRequestViewModel> GetRefundRequestAsync(int paymentId)
    {
        var payment = await _unitOfWork.CustomerPayments.GetPaymentByIdAsync(paymentId);
        if (payment is null)
        {
            return null;
        }

        var model = _mapper.Map<PaymentRefundRequestViewModel>(payment);
        model.Refunds = _mapper.Map<List<PaymentRefundViewModel>>(await _unitOfWork.CustomerPayments.GetRefundsForPaymentAsync(paymentId));
        model.RemainingRefundable = payment.Paid - model.Refunds.Where(a => a.Status != RefundStatus.Rejected).Sum(a => a.Amount);

        return model;
    }

    public async Task<ResultMessage> RequestRefundAsync(PaymentRefundRequestViewModel model, string requestedByUserId)
    {
        var entity = _mapper.Map<PaymentRefund>(model);

        return await _unitOfWork.CustomerPayments.RequestRefundAsync(entity, requestedByUserId);
    }

    public async Task<ResultMessage> ApproveRefundAsync(int refundId, string approvedByUserId)
    {
        return await _unitOfWork.CustomerPayments.ApproveRefundAsync(refundId, approvedByUserId);
    }

    public async Task<ResultMessage> RejectRefundAsync(int refundId, string approvedByUserId)
    {
        return await _unitOfWork.CustomerPayments.RejectRefundAsync(refundId, approvedByUserId);
    }
}
