using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ErpEntryService : IErpEntryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ErpEntryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<ErpEntryViewModel>> GetPagedEntriesAsync(PageRequest request)
    {
        return await _unitOfWork.ErpEntries.GetPagedEntriesAsync(request);
    }

    public async Task<ErpEntryViewModel> GetEntryByIdAsync(int id)
    {
        return await _unitOfWork.ErpEntries.GetEntryByIdAsync(id);
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        return await _unitOfWork.ErpEntries.GetPostingAccountOptionsAsync();
    }

    public async Task<ResultMessage> CreateManualEntryAsync(ErpEntryViewModel model)
    {
        return await _unitOfWork.ErpEntries.CreateManualEntryAsync(model);
    }

    public async Task<ResultMessage> DeleteManualEntryAsync(int id)
    {
        return await _unitOfWork.ErpEntries.DeleteManualEntryAsync(id);
    }
}
