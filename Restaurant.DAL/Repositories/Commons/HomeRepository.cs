using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Commons;
using Restaurant.DAL.Models;

namespace Restaurant.DAL.Repositories.Commons;

public class HomeRepository : IHomeRepository
{
    private readonly ApplicationDbContext _context;

    public HomeRepository(ApplicationDbContext context)
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

    public async Task<ApplicationSetting> GetSettingsAsync()
    {
        return await _context.ApplicationSettings
            .AsNoTracking()
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MenuItem>> GetMenuItemsAsync(int? sectionId)
    {
        return await _context.MenuItems
            .AsNoTracking()
            .Include(a => a.Components).ThenInclude(a => a.Product)
            .Where(a => a.ShowOnWebsite && (sectionId == null || a.MenuSectionId == sectionId))
            .OrderBy(a => a.MenuSectionId)
            .ThenBy(a => a.ArName)
            .ThenBy(a => a.Size)
            .ToListAsync();
    }
}
