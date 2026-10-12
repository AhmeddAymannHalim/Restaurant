using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IProductCategoryService
{
    Task<PaginatedResult<ProductCategoryViewModel>> GetPagedProductCategoriesAsync(PageRequest request);
    Task<ProductCategoryViewModel> GetProductCategoryByIdAsync(int id);
    Task<ResultMessage> UpsertProductCategoryAsync(ProductCategoryViewModel model);
    Task<ResultMessage> DeleteProductCategoryAsync(int id);
}
