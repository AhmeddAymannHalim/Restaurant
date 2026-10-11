using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(string userId)
    {
        return await _unitOfWork.Dashboard.GetDashboardAsync(userId);
    }
}
