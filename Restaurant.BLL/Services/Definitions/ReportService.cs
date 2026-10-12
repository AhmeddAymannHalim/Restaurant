using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReportViewModel> BuildReportAsync(ReportRequest request)
    {
        return await _unitOfWork.Reports.BuildAsync(request);
    }
}
