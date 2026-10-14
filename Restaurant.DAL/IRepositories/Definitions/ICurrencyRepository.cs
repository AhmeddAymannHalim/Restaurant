using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface ICurrencyRepository
{
    Task<PaginatedResult<Currency>> GetPagedCurrenciesAsync(PageRequest request);
    Task<Currency> GetCurrencyByIdAsync(int id);
    Task<string> NewCurrencyCodeAsync();
    Task<ResultMessage> UpsertCurrencyAsync(Currency entity);
    Task<ResultMessage> DeleteCurrencyAsync(int id);
}
