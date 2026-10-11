using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IDashboardRepository
{
    Task<DashboardViewModel> GetDashboardAsync(string userId);
}
