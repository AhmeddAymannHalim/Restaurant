using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<InvoiceViewModel>> GetPagedInvoicesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Invoices.GetPagedInvoicesAsync(request);

        return PaginatedResult<InvoiceViewModel>.Create(_mapper.Map<List<InvoiceViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<InvoiceViewModel> GetInvoiceByIdAsync(int id)
    {
        var entity = await _unitOfWork.Invoices.GetInvoiceByIdAsync(id);

        return _mapper.Map<InvoiceViewModel>(entity);
    }

    public async Task<string> GetNextCodeAsync()
    {
        return await _unitOfWork.Invoices.GetNextCodeAsync();
    }

    public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
    {
        return await _unitOfWork.Invoices.GetSupplierBalanceAsync(supplierId);
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        return await _unitOfWork.Invoices.GetCustomerBalanceAsync(customerId);
    }

    public async Task<ResultMessage> UpsertInvoiceAsync(InvoiceViewModel model)
    {
        return await _unitOfWork.Invoices.UpsertInvoiceAsync(_mapper.Map<Invoice>(model));
    }

    public async Task<ResultMessage> DeleteInvoiceAsync(int id)
    {
        return await _unitOfWork.Invoices.DeleteInvoiceAsync(id);
    }
}
