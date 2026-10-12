using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IReportRepository
{
    Task<ReportViewModel> BuildAsync(ReportRequest request);
}
