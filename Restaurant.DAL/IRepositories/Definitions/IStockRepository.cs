using Restaurant.DAL.Helpers;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IStockRepository
{
    Task<int> BaseUnitIdAsync();
    Task<StoreItemUnit> RowAsync(int storeId, int productId);
    Task<StoreItemUnit> FindAsync(int storeId, int productId);
    Task<bool> AdjustAsync(int storeId, int productId, decimal delta, bool allowNegative = false, bool createIfMissing = true);
    IQueryable<StoreItemUnit> Rows();
    Task<List<StockQuantity>> QuantitiesAsync(int? storeId = null);
    decimal FindQuantity(List<StockQuantity> quantities, int storeId, int productId);
}
