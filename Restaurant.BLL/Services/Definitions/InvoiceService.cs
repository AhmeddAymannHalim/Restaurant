using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceViewModel> GetInvoiceByIdAsync(int id)
    {
        return await _unitOfWork.Invoices.GetInvoiceByIdAsync(id);
    }

    public async Task<List<PostingDefaultViewModel>> GetPostingDefaultsAsync()
    {
        return await _unitOfWork.Invoices.GetPostingDefaultsAsync();
    }

    public async Task<InvoiceViewModel> GetNewInvoiceAsync(InvoiceType type)
    {
        return await _unitOfWork.Invoices.GetNewInvoiceAsync(type);
    }

    public async Task<string> GetNextCodeAsync(InvoiceType type)
    {
        return await _unitOfWork.Invoices.GetNextCodeAsync(type);
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
        return await _unitOfWork.Invoices.UpsertInvoiceAsync(model);
    }

    public async Task<ResultMessage> DeleteInvoiceAsync(int id)
    {
        return await _unitOfWork.Invoices.DeleteInvoiceAsync(id);
    }
}
