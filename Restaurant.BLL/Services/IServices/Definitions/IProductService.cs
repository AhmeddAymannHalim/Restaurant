using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IProductService
{
    Task<PaginatedResult<ProductViewModel>> GetPagedProductsAsync(PageRequest request);
    Task<ProductViewModel> GetProductByIdAsync(int id);
    Task<ResultMessage> UpsertProductAsync(ProductViewModel model);
    Task<ResultMessage> DeleteProductAsync(int id);
}
