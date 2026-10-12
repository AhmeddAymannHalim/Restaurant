using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Commons;

public interface IHomeService
{
    Task<HomeViewModel> GetHomeAsync();
    Task<IEnumerable<HomeMenuItemViewModel>> GetMenuItemsAsync(int? sectionId);
    Task<SitePricingViewModel> GetPricingAsync();
}
