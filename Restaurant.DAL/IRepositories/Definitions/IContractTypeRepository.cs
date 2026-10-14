using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IContractTypeRepository
{
    Task<PaginatedResult<ContractType>> GetPagedContractTypesAsync(PageRequest request);
    Task<ContractType> GetContractTypeByIdAsync(int id);
    Task<ResultMessage> UpsertContractTypeAsync(ContractType entity);
    Task<ResultMessage> DeleteContractTypeAsync(int id);
}
