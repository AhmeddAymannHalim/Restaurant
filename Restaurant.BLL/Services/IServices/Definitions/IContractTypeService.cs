using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IContractTypeService
{
    Task<PaginatedResult<ContractTypeViewModel>> GetPagedContractTypesAsync(PageRequest request);
    Task<ContractTypeViewModel> GetContractTypeByIdAsync(int id);
    Task<ResultMessage> UpsertContractTypeAsync(ContractTypeViewModel model);
    Task<ResultMessage> DeleteContractTypeAsync(int id);
    Task<string> NewContractTypeCodeAsync();
}
