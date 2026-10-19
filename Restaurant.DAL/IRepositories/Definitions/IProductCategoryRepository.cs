using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IProductCategoryRepository
{
    Task<PaginatedResult<ProductCategory>> GetPagedProductCategoriesAsync(PageRequest request);
    Task<ProductCategory> GetProductCategoryByIdAsync(int id);
    Task<ResultMessage> UpsertProductCategoryAsync(ProductCategory entity);
    Task<string> NewProductCategoryCodeAsync();
    Task<ResultMessage> DeleteProductCategoryAsync(int id);
}
