using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IJobTypeService
{
    Task<PaginatedResult<JobTypeViewModel>> GetPagedJobTypesAsync(PageRequest request);
    Task<JobTypeViewModel> GetJobTypeByIdAsync(int id);
    Task<ResultMessage> UpsertJobTypeAsync(JobTypeViewModel model);
    Task<ResultMessage> DeleteJobTypeAsync(int id);
}
