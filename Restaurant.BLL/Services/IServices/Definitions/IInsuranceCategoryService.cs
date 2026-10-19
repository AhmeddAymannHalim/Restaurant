using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IInsuranceCategoryService
{
    Task<PaginatedResult<InsuranceCategoryViewModel>> GetPagedInsuranceCategoriesAsync(PageRequest request);
    Task<InsuranceCategoryViewModel> GetInsuranceCategoryByIdAsync(int id);
    Task<ResultMessage> UpsertInsuranceCategoryAsync(InsuranceCategoryViewModel model);
    Task<ResultMessage> DeleteInsuranceCategoryAsync(int id);
    Task<string> NewInsuranceCategoryCodeAsync();
}
