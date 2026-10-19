using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class MenuSectionService : IMenuSectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MenuSectionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<MenuSectionViewModel>> GetPagedMenuSectionsAsync(PageRequest request)
    {
        var result = await _unitOfWork.MenuSections.GetPagedMenuSectionsAsync(request);

        return PaginatedResult<MenuSectionViewModel>.Create(_mapper.Map<List<MenuSectionViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<MenuSectionViewModel> GetMenuSectionByIdAsync(int id)
    {
        var entity = await _unitOfWork.MenuSections.GetMenuSectionByIdAsync(id);

        return _mapper.Map<MenuSectionViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertMenuSectionAsync(MenuSectionViewModel model)
    {
        var entity = _mapper.Map<MenuSection>(model);

        return await _unitOfWork.MenuSections.UpsertMenuSectionAsync(entity);
    }

    public async Task<ResultMessage> DeleteMenuSectionAsync(int id)
    {
        return await _unitOfWork.MenuSections.DeleteMenuSectionAsync(id);
    }

    public async Task<string> NewMenuSectionCodeAsync()
    {
        return await _unitOfWork.MenuSections.NewMenuSectionCodeAsync();
    }
}
