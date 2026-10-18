using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IDeductionTypeRepository
{
    Task<PaginatedResult<DeductionType>> GetPagedDeductionTypesAsync(PageRequest request);
    Task<DeductionType> GetDeductionTypeByIdAsync(int id);
    Task<ResultMessage> UpsertDeductionTypeAsync(DeductionType entity);
    Task<ResultMessage> DeleteDeductionTypeAsync(int id);
}
