using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class BillService : IBillService
{
    private readonly IUnitOfWork _unitOfWork;

    public BillService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<BillListItemViewModel>> GetPagedBillsAsync(PageRequest request)
    {
        return await _unitOfWork.Bills.GetPagedBillsAsync(request);
    }

    public async Task<BillViewModel> GetBillByIdAsync(int id)
    {
        return await _unitOfWork.Bills.GetBillByIdAsync(id);
    }

    public async Task<ResultMessage> PostBillAsync(int id)
    {
        if (await _unitOfWork.Invoices.IsInvoiceAsync(id))
        {
            return await _unitOfWork.Invoices.PostInvoiceAsync(id);
        }

        return await _unitOfWork.Bills.PostBillAsync(id);
    }

    public async Task<ResultMessage> PostPendingAsync()
    {
        var posted = 0;
        foreach (var id in await _unitOfWork.Invoices.GetUnpostedInvoiceIdsAsync())
        {
            var result = await _unitOfWork.Invoices.PostInvoiceAsync(id);
            if (!result.Success)
            {
                result.Data = posted.ToString();
                return result;
            }

            posted++;
        }

        var cashier = await _unitOfWork.Bills.PostPendingAsync();
        if (cashier.Success)
        {
            cashier.Data = (posted + int.Parse(cashier.Data?.ToString() ?? "0")).ToString();
        }

        return cashier;
    }
}
