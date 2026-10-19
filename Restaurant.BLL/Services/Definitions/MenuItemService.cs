using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class MenuItemService : IMenuItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MenuItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<MenuItemViewModel>> GetPagedMenuItemsAsync(PageRequest request)
    {
        var result = await _unitOfWork.MenuItems.GetPagedMenuItemsAsync(request);

        return PaginatedResult<MenuItemViewModel>.Create(_mapper.Map<List<MenuItemViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<MenuItemViewModel> GetMenuItemByIdAsync(int id)
    {
        var entity = await _unitOfWork.MenuItems.GetMenuItemByIdAsync(id);

        return _mapper.Map<MenuItemViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertMenuItemAsync(MenuItemViewModel model)
    {
        var entity = _mapper.Map<MenuItem>(model);

        return await _unitOfWork.MenuItems.UpsertMenuItemAsync(entity, model.ImageFile);
    }

    public async Task<ResultMessage> DeleteMenuItemAsync(int id)
    {
        return await _unitOfWork.MenuItems.DeleteMenuItemAsync(id);
    }

    public async Task<string> NewMenuItemCodeAsync()
    {
        return await _unitOfWork.MenuItems.NewMenuItemCodeAsync();
    }
}
