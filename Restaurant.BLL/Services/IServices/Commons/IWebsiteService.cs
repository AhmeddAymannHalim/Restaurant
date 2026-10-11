using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Commons;

public interface IWebsiteService
{
    Task<WebsiteViewModel> GetWebsiteAsync();
    Task<IEnumerable<WebsiteMenuItemViewModel>> GetMenuItemsAsync(int? sectionId);
}
