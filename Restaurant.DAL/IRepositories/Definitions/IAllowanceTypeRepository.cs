using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IAllowanceTypeRepository
{
    Task<PaginatedResult<AllowanceType>> GetPagedAllowanceTypesAsync(PageRequest request);
    Task<AllowanceType> GetAllowanceTypeByIdAsync(int id);
    Task<ResultMessage> UpsertAllowanceTypeAsync(AllowanceType entity);
    Task<ResultMessage> DeleteAllowanceTypeAsync(int id);
}
