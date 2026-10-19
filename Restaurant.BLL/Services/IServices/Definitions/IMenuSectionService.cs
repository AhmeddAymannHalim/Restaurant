using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IMenuSectionService
{
    Task<PaginatedResult<MenuSectionViewModel>> GetPagedMenuSectionsAsync(PageRequest request);
    Task<MenuSectionViewModel> GetMenuSectionByIdAsync(int id);
    Task<ResultMessage> UpsertMenuSectionAsync(MenuSectionViewModel model);
    Task<ResultMessage> DeleteMenuSectionAsync(int id);
    Task<string> NewMenuSectionCodeAsync();
}
