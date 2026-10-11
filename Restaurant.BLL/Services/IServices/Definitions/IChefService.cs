using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IChefService
{
    Task<PaginatedResult<ChefViewModel>> GetPagedChefsAsync(PageRequest request);
    Task<ChefViewModel> GetChefByIdAsync(int id);
    Task<ResultMessage> UpsertChefAsync(ChefViewModel model);
    Task<ResultMessage> DeleteChefAsync(int id);
}
