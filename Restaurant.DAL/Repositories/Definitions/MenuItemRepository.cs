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
        var query = _context.MenuItems
            .Include(a => a.MenuSection)
            .Include(a => a.Store)
            .Include(a => a.Components)
                .ThenInclude(a => a.Product)
            .AsNoTracking();

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

    public async Task<MenuItem> GetMenuItemByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(a => a.MenuSection)
            .Include(a => a.Store)
            .Include(a => a.Components)
                .ThenInclude(a => a.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ResultMessage> UpsertMenuItemAsync(MenuItem entity, IFormFile imageFile)
    {
        try
        {
            entity.Components = (entity.Components ?? new List<MenuItemComponent>())
                .Where(a => a.ProductId > 0 && a.Quantity > 0)
                .GroupBy(a => a.ProductId)
                .Select(a => new MenuItemComponent
                    {
                        ProductId = a.Key,
                        Quantity = a.Sum(x => x.Quantity)
                    })
                .ToList();

            if (entity.Components.Count == 0)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "MenuItemNeedsComponents"
                };
            }

            var productIds = entity.Components.Select(a => a.ProductId).ToList();
            if (await _context.Products.CountAsync(a => productIds.Contains(a.Id)) != productIds.Count)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            entity.Size = entity.Size == Size.regular ? null : entity.Size;

            var duplicate = await _context.MenuItems.AnyAsync(a => a.Id != entity.Id
                && a.ArName == entity.ArName
                && a.Size == entity.Size);
            if (duplicate)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "MenuItemSizeExists"
                };
            }

            if (entity.Id == 0)
            {
                if (imageFile != null)
                {
                    entity.ImageUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                await _context.MenuItems.AddAsync(entity);
            }
            else
            {
                var existing = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                entity.ImageUrl = existing.ImageUrl;

                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        DocumentSetting.DeleteFile(existing.ImageUrl);
                    }

                    entity.ImageUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                _context.MenuItemComponents.RemoveRange(_context.MenuItemComponents.Where(a => a.MenuItemId == entity.Id));
                _context.MenuItems.Update(entity);
            }

            await _context.SaveChangesAsync();

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }

    public async Task<ResultMessage> DeleteMenuItemAsync(int id)
    {
        try
        {
            var entity = await _context.MenuItems.FindAsync(id);
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
            return new ResultMessage
            {
                Success = false,
                Message = "CannotDeleteLinkedRecord"
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ErrorLog.Message(ex)
            };
        }
    }
}
