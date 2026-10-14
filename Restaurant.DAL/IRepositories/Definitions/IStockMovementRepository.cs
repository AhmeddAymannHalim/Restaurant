using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IStockMovementRepository
{
    Task<PaginatedResult<StoreProductMovement>> GetPagedAsync(PageRequest request, int? storeId, int? productId);
}
