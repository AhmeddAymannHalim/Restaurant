using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(string userId);
}
