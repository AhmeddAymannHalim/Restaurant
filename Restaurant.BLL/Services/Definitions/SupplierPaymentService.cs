using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
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
}
