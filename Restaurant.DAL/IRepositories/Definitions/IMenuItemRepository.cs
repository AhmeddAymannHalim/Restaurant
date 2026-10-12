using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IMenuItemRepository
{
    Task<PaginatedResult<MenuItem>> GetPagedMenuItemsAsync(PageRequest request);
    Task<MenuItem> GetMenuItemByIdAsync(int id);
    Task<ResultMessage> UpsertMenuItemAsync(MenuItem entity, IFormFile imageFile);
    Task<ResultMessage> DeleteMenuItemAsync(int id);
}
