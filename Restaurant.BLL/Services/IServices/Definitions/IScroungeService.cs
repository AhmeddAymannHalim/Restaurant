using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IScroungeService
{
    Task<PaginatedResult<ScroungeViewModel>> GetPagedScroungesAsync(PageRequest request);
    Task<ScroungeViewModel> GetScroungeByIdAsync(int id);
    Task<ResultMessage> UpsertScroungeAsync(ScroungeViewModel model, string userId);
    Task<ResultMessage> DeleteScroungeAsync(int id);
}
