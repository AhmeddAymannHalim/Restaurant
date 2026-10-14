using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface ICurrencyService
{
    Task<PaginatedResult<CurrencyViewModel>> GetPagedCurrenciesAsync(PageRequest request);
    Task<CurrencyViewModel> GetCurrencyByIdAsync(int id);
    Task<ResultMessage> UpsertCurrencyAsync(CurrencyViewModel model);
    Task<ResultMessage> DeleteCurrencyAsync(int id);
    Task<string> NewCurrencyCodeAsync();
}
