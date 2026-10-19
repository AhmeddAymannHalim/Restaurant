using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IJobTypeRepository
{
    Task<PaginatedResult<JobType>> GetPagedJobTypesAsync(PageRequest request);
    Task<JobType> GetJobTypeByIdAsync(int id);
    Task<ResultMessage> UpsertJobTypeAsync(JobType entity);
    Task<string> NewJobTypeCodeAsync();
    Task<ResultMessage> DeleteJobTypeAsync(int id);
}
