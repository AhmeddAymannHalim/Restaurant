using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CurrencyService : ICurrencyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CurrencyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CurrencyViewModel>> GetPagedCurrenciesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Currencies.GetPagedCurrenciesAsync(request);

        return PaginatedResult<CurrencyViewModel>.Create(_mapper.Map<List<CurrencyViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<CurrencyViewModel> GetCurrencyByIdAsync(int id)
    {
        var entity = await _unitOfWork.Currencies.GetCurrencyByIdAsync(id);

        return _mapper.Map<CurrencyViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertCurrencyAsync(CurrencyViewModel model)
    {
        var entity = _mapper.Map<Currency>(model);

        return await _unitOfWork.Currencies.UpsertCurrencyAsync(entity);
    }

    public async Task<ResultMessage> DeleteCurrencyAsync(int id)
    {
        return await _unitOfWork.Currencies.DeleteCurrencyAsync(id);
    }

    public async Task<string> NewCurrencyCodeAsync()
    {
        return await _unitOfWork.Currencies.NewCurrencyCodeAsync();
    }
}
