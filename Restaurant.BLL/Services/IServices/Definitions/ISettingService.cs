using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISettingService
{
    Task<ApplicationSettingViewModel> GetApplicationSettingAsync();
    Task<ResultMessage> UpdateApplicationSettingAsync(ApplicationSettingViewModel model);

    Task<RestaurantSettingViewModel> GetRestaurantSettingAsync();
    Task<ResultMessage> UpdateRestaurantSettingAsync(RestaurantSettingViewModel model);

    Task<PrinterSettingViewModel> GetPrinterSettingAsync();
    Task<ResultMessage> UpdatePrinterSettingAsync(PrinterSettingViewModel model);
}
