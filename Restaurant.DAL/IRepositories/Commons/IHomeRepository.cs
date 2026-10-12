using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Commons;

public interface IWebsiteRepository
{
    Task<IndexSettings> GetIndexSettingsAsync();
    Task<List<ChefItem>> GetChefsAsync();
    Task<List<GalleryItem>> GetGalleryAsync();
    Task<List<MenuSection>> GetMenuSectionsAsync();
    Task<List<MenuItem>> GetMenuItemsAsync(int? sectionId);
}
