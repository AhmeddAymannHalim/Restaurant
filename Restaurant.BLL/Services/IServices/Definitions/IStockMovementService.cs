using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IStockMovementService
{
    Task<PaginatedResult<StockMovementViewModel>> GetPagedAsync(PageRequest request, int? storeId, int? productId);
}
