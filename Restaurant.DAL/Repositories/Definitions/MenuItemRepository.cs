using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class MenuItemRepository : IMenuItemRepository
{
    private const string ImagesFolder = "Menu Images";

    private readonly ApplicationDbContext _context;

    public MenuItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<MenuItem>> GetPagedMenuItemsAsync(PageRequest request)
    {
        var query = _context.MenuItems.AsNoTracking().Include(a => a.MenuSection).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search) || a.EnName.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<MenuItem>.Create(data, total, request);
    }

    // The item together with its components (the Upsert screen edits them as one unit).
    public async Task<MenuItem> GetMenuItemByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(a => a.Components)
                .ThenInclude(a => a.StoreProduct)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    // "Add a size": a new, unsaved item that starts as a copy of an existing one.
    public async Task<MenuItem> GetNewSizeFromAsync(int baseId)
    {
        var entity = await _context.MenuItems
            .AsNoTracking()
            .Include(a => a.Components)
                .ThenInclude(a => a.StoreProduct)
            .FirstOrDefaultAsync(a => a.Id == baseId);
        if (entity is null)
        {
            return null;
        }

        entity.Id = 0;
        entity.Size = null;
        foreach (var component in entity.Components)
        {
            component.Id = 0;
        }

        return entity;
    }

    public async Task<ResultMessage> UpsertMenuItemAsync(MenuItem entity, IFormFile imageFile)
    {
        try
        {
            var components = (entity.Components ?? new List<MenuItemComponent>())
                .Where(a => a.StoreProductId > 0 && a.Quantity > 0)
                .ToList();
            if (components.Count == 0)
            {
                return new ResultMessage { Success = false, Message = "MenuItemNeedsComponents" };
            }

            entity.Size = entity.Size == Size.regular ? null : entity.Size;

            var duplicate = await _context.MenuItems.AsNoTracking()
                .AnyAsync(a => a.Id != entity.Id && a.ArName == entity.ArName && a.Size == entity.Size);
            if (duplicate)
            {
                return new ResultMessage { Success = false, Message = "MenuItemSizeExists" };
            }

            var ids = components.Select(a => a.StoreProductId).Distinct().ToList();
            var prices = await _context.StoreProducts.AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.PriceForUnit);
            if (ids.Any(a => !prices.ContainsKey(a)))
            {
                return new ResultMessage { Success = false, Message = "RecordNotFound" };
            }

            var newComponents = components.Select(a => new MenuItemComponent
            {
                StoreProductId = a.StoreProductId,
                Quantity = a.Quantity,
                Cost = Math.Round(a.Quantity * prices[a.StoreProductId], 2)
            }).ToList();

            entity.TotalCost = newComponents.Sum(a => a.Cost);
            entity.Benefit = entity.Price - entity.TotalCost;
            entity.Components = newComponents;

            if (entity.Id == 0)
            {
                // A new size keeps the picture it was posted with unless a new file comes in
                if (imageFile != null)
                {
                    entity.ImageUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                entity.IsReady = true;
                await _context.MenuItems.AddAsync(entity);
            }
            else
            {
                var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage { Success = false, Message = "RecordNotFound" };
                }

                entity.IsReady = existing.IsReady;
                entity.ImageUrl = existing.ImageUrl;

                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        DocumentSetting.DeleteFile(existing.ImageUrl);
                    }

                    entity.ImageUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                // The components are replaced as a whole
                var oldComponents = await _context.Set<MenuItemComponent>().Where(a => a.MenuItemId == entity.Id).ToListAsync();
                _context.Set<MenuItemComponent>().RemoveRange(oldComponents);
                _context.MenuItems.Update(entity);
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }

    public async Task<ResultMessage> DeleteMenuItemAsync(int id)
    {
        try
        {
            var entity = await _context.MenuItems.Include(a => a.Components).FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (DbUpdateException)
        {
            return new ResultMessage { Success = false, Message = "CannotDeleteLinkedRecord" };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ex.Message };
        }
    }
}
