using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SettingService : ISettingService
{
    private readonly ISettingRepository _repository;
    private readonly IMapper _mapper;

    public SettingService(ISettingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApplicationSettingViewModel> GetApplicationSettingAsync()
    {
        var entity = await _repository.GetApplicationSettingAsync();

        return _mapper.Map<ApplicationSettingViewModel>(entity);
    }

    public async Task<ResultMessage> UpdateApplicationSettingAsync(ApplicationSettingViewModel model)
    {
        return await _repository.UpdateApplicationSettingAsync(_mapper.Map<ApplicationSetting>(model));
    }

    public async Task<RestaurantSettingViewModel> GetRestaurantSettingAsync()
    {
        var entity = await _repository.GetRestaurantSettingAsync();

        return _mapper.Map<RestaurantSettingViewModel>(entity);
    }

    public async Task<ResultMessage> UpdateRestaurantSettingAsync(RestaurantSettingViewModel model)
    {
        return await _repository.UpdateRestaurantSettingAsync(_mapper.Map<IndexSettings>(model), model.Image, model.Video);
    }

    public async Task<PrinterSettingViewModel> GetPrinterSettingAsync()
    {
        var entity = await _repository.GetPrinterSettingAsync();

        return _mapper.Map<PrinterSettingViewModel>(entity);
    }

    public async Task<ResultMessage> UpdatePrinterSettingAsync(PrinterSettingViewModel model)
    {
        return await _repository.UpdatePrinterSettingAsync(_mapper.Map<PrinterSetting>(model));
    }
}
