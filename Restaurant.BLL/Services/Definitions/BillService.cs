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
        return await _unitOfWork.Bills.PostBillAsync(id);
    }

    public async Task<ResultMessage> PostPendingAsync()
    {
        return await _unitOfWork.Bills.PostPendingAsync();
    }
}
