using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IJobService
{
    Task<PaginatedResult<JobViewModel>> GetPagedJobsAsync(PageRequest request);
    Task<JobViewModel> GetJobByIdAsync(int id);
    Task<ResultMessage> UpsertJobAsync(JobViewModel model);
    Task<ResultMessage> DeleteJobAsync(int id);
    Task<string> NewJobCodeAsync();
}
