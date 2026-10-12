using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IStoreConvertRepository
{
    Task<PaginatedResult<StoreConvert>> GetPagedConvertsAsync(PageRequest request);
    Task<StoreConvert> GetConvertByIdAsync(int id);
    Task<string> GetNextCodeAsync();
    Task<ResultMessage> CreateConvertAsync(StoreConvert convert);
    Task<ResultMessage> DeleteConvertAsync(int id);
}
