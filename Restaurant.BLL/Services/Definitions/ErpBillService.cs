using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ErpBillService : IErpBillService
{
    private readonly IUnitOfWork _unitOfWork;

    public ErpBillService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<ErpBillListItemViewModel>> GetPagedBillsAsync(PageRequest request)
    {
        return await _unitOfWork.ErpBills.GetPagedBillsAsync(request);
    }

    public async Task<ErpBillViewModel> GetBillByIdAsync(int id)
    {
        return await _unitOfWork.ErpBills.GetBillByIdAsync(id);
    }

    public async Task<ResultMessage> PostBillAsync(int id)
    {
        return await _unitOfWork.ErpBills.PostBillAsync(id);
    }

    public async Task<ResultMessage> PostPendingAsync()
    {
        return await _unitOfWork.ErpBills.PostPendingAsync();
    }
}
