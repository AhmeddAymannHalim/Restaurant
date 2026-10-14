using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ISpecializationService
{
    Task<PaginatedResult<SpecializationViewModel>> GetPagedSpecializationsAsync(PageRequest request);
    Task<SpecializationViewModel> GetSpecializationByIdAsync(int id);
    Task<ResultMessage> UpsertSpecializationAsync(SpecializationViewModel model);
    Task<ResultMessage> DeleteSpecializationAsync(int id);
    Task<string> NewSpecializationCodeAsync();
}
