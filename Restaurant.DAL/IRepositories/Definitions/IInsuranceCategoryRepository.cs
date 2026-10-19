using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IInsuranceCategoryRepository
{
    Task<PaginatedResult<InsuranceCategory>> GetPagedInsuranceCategoriesAsync(PageRequest request);
    Task<InsuranceCategory> GetInsuranceCategoryByIdAsync(int id);
    Task<ResultMessage> UpsertInsuranceCategoryAsync(InsuranceCategory entity);
    Task<string> NewInsuranceCategoryCodeAsync();
    Task<ResultMessage> DeleteInsuranceCategoryAsync(int id);
}
