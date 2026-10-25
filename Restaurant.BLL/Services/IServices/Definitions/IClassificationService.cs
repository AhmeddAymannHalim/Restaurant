using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IClassificationService
{
    Task<PaginatedResult<ClassificationViewModel>> GetPagedClassificationsAsync(PageRequest request);
    Task<ClassificationViewModel> GetClassificationByIdAsync(int id);
    Task<ResultMessage> UpsertClassificationAsync(ClassificationViewModel model);
    Task<ResultMessage> DeleteClassificationAsync(int id);
}
