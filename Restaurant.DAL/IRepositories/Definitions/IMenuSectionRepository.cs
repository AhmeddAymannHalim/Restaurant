using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IMenuSectionRepository
{
    Task<PaginatedResult<MenuSection>> GetPagedMenuSectionsAsync(PageRequest request);
    Task<MenuSection> GetMenuSectionByIdAsync(int id);
    Task<ResultMessage> UpsertMenuSectionAsync(MenuSection entity);
    Task<ResultMessage> DeleteMenuSectionAsync(int id);
}
