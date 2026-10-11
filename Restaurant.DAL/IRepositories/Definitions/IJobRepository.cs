using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IJobRepository
{
    Task<PaginatedResult<Job>> GetPagedJobsAsync(PageRequest request);
    Task<Job> GetJobByIdAsync(int id);
    Task<ResultMessage> UpsertJobAsync(Job entity);
    Task<ResultMessage> DeleteJobAsync(int id);
    Task<string> NewJobCodeAsync();
}
