using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IMenuItemService
{
    Task<PaginatedResult<MenuItemViewModel>> GetPagedMenuItemsAsync(PageRequest request);
    Task<MenuItemViewModel> GetMenuItemByIdAsync(int id);
    Task<ResultMessage> UpsertMenuItemAsync(MenuItemViewModel model);
    Task<ResultMessage> DeleteMenuItemAsync(int id);
}
