using AutoMapper;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Commons;

public class WebsiteService : IWebsiteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WebsiteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WebsiteViewModel> GetWebsiteAsync()
    {
        var model = _mapper.Map<WebsiteViewModel>(await _unitOfWork.Website.GetIndexSettingsAsync()) ?? new WebsiteViewModel();
        model.MenuSections = _mapper.Map<List<WebsiteMenuSectionViewModel>>(await _unitOfWork.Website.GetMenuSectionsAsync());
        model.Chefs = _mapper.Map<List<ChefViewModel>>(await _unitOfWork.Website.GetChefsAsync());
        model.Gallery = _mapper.Map<List<GalleryViewModel>>(await _unitOfWork.Website.GetGalleryAsync());

        return model;
    }

    public async Task<IEnumerable<WebsiteMenuItemViewModel>> GetMenuItemsAsync(int? sectionId)
    {
        return _mapper.Map<List<WebsiteMenuItemViewModel>>(await _unitOfWork.Website.GetMenuItemsAsync(sectionId));
    }
}
