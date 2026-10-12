using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Repositories.Commons;

public class WebsiteRepository : IWebsiteRepository
{
    private readonly ApplicationDbContext _context;

    public WebsiteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IndexSettings> GetIndexSettingsAsync()
    {
        return await _context.IndexSettings
            .AsNoTracking()
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ChefItem>> GetChefsAsync()
    {
        return await _context.ChefItem.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }

    public async Task<List<GalleryItem>> GetGalleryAsync()
    {
        return await _context.GalleryItem.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
    }

    // Only the categories that have something to order
    public async Task<List<MenuSection>> GetMenuSectionsAsync()
    {
        return await _context.MenuSections
            .AsNoTracking()
            .Where(a => a.MenuItems.Any(i => i.IsReady))
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<List<MenuItem>> GetMenuItemsAsync(int? sectionId)
    {
        return await _context.MenuItems
            .AsNoTracking()
            .Where(a => a.IsReady && (sectionId == null || a.MenuSectionId == sectionId))
            .OrderBy(a => a.MenuSectionId)
            .ThenBy(a => a.ArName)
            .ThenBy(a => a.Size)
            .ToListAsync();
    }
}
