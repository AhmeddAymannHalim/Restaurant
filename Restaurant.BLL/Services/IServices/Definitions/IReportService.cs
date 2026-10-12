using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IReportService
{
    Task<ReportViewModel> BuildReportAsync(ReportRequest request);
}
