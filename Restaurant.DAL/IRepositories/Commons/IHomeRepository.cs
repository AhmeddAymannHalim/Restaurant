using Restaurant.DAL.Models;

namespace Restaurant.DAL.IRepositories.Commons;

public interface IHomeRepository
{
    Task<IndexSettings> GetIndexSettingsAsync();
    Task<List<ChefItem>> GetChefsAsync();
    Task<List<GalleryItem>> GetGalleryAsync();
    Task<List<MenuItem>> GetMenuItemsAsync(int? sectionId);
    Task<ApplicationSetting> GetSettingsAsync();
}
