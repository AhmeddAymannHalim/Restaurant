using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IProductRepository
{
    Task<PaginatedResult<Product>> GetPagedProductsAsync(PageRequest request);
    Task<Product> GetProductByIdAsync(int id);
    Task<ResultMessage> UpsertProductAsync(Product entity);
    Task<ResultMessage> DeleteProductAsync(int id);
}
