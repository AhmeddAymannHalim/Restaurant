using AutoMapper;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Commons;

public class HomeService : IHomeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HomeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<HomeViewModel> GetHomeAsync()
    {
        var model = _mapper.Map<HomeViewModel>(await _unitOfWork.Home.GetIndexSettingsAsync()) ?? new HomeViewModel();
        model.MenuSections = (await _unitOfWork.Lookups.ReadyMenuSectionLookupAsync()).ToList();
        model.Chefs = _mapper.Map<List<ChefViewModel>>(await _unitOfWork.Home.GetChefsAsync());
        model.Gallery = _mapper.Map<List<GalleryViewModel>>(await _unitOfWork.Home.GetGalleryAsync());

        return model;
    }

    public async Task<SitePricingViewModel> GetPricingAsync()
    {
        var settings = await _unitOfWork.Home.GetSettingsAsync();

        return new SitePricingViewModel
        {
            TaxPercent = settings?.OrderTax ?? 0,
            Service = settings?.OrderService ?? 0,
            Zones = _mapper.Map<List<DeliveryZoneViewModel>>(await _unitOfWork.DeliveryZones.GetAllDeliveryZonesAsync())
        };
    }

    public async Task<IEnumerable<HomeMenuItemViewModel>> GetMenuItemsAsync(int? sectionId)
    {
        return _mapper.Map<List<HomeMenuItemViewModel>>(await _unitOfWork.Home.GetMenuItemsAsync(sectionId));
    }
}
