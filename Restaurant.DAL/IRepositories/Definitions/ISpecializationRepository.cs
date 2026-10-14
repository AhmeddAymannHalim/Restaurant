using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ISpecializationRepository
{
    Task<PaginatedResult<Specialization>> GetPagedSpecializationsAsync(PageRequest request);
    Task<Specialization> GetSpecializationByIdAsync(int id);
    Task<string> NewSpecializationCodeAsync();
    Task<ResultMessage> UpsertSpecializationAsync(Specialization entity);
    Task<ResultMessage> DeleteSpecializationAsync(int id);
}
