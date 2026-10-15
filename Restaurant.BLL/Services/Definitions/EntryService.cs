using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class EntryService : IEntryService
{
    private readonly IUnitOfWork _unitOfWork;

    public EntryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<EntryViewModel>> GetPagedEntriesAsync(PageRequest request)
    {
        return await _unitOfWork.Entries.GetPagedEntriesAsync(request);
    }

    public async Task<EntryViewModel> GetEntryByIdAsync(int id)
    {
        return await _unitOfWork.Entries.GetEntryByIdAsync(id);
    }

    public async Task<IEnumerable<LookupItem>> GetPostingAccountOptionsAsync()
    {
        return await _unitOfWork.Entries.GetPostingAccountOptionsAsync();
    }

    public async Task<ResultMessage> CreateManualEntryAsync(EntryViewModel model)
    {
        return await _unitOfWork.Entries.CreateManualEntryAsync(model);
    }

    public async Task<ResultMessage> DeleteManualEntryAsync(int id)
    {
        return await _unitOfWork.Entries.DeleteManualEntryAsync(id);
    }
}
