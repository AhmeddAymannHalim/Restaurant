using Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class MenuSectionRepository : IMenuSectionRepository
{
    private readonly ApplicationDbContext _context;

    public MenuSectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<MenuSection>> GetPagedMenuSectionsAsync(PageRequest request)
    {
        var query = _context.MenuSections.AsNoTracking().Include(a => a.Kitchen).AsQueryable();

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

        return PaginatedResult<MenuSection>.Create(data, total, request);
    }

    public async Task<MenuSection> GetMenuSectionByIdAsync(int id)
    {
        return await _context.MenuSections.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertMenuSectionAsync(MenuSection entity)
    {
        try
        {
            if (entity.Id == 0)
            {
                await _context.MenuSections.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.MenuSections.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
                if (existing is null)
                {
                    return new ResultMessage
                    {
                        Success = false,
                        Message = "RecordNotFound"
                    };
                }

                _context.MenuSections.Update(entity);
                await _context.SaveChangesAsync();
            }

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

    public async Task<ResultMessage> DeleteMenuSectionAsync(int id)
    {
        try
        {
            var entity = await _context.MenuSections.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.MenuSections.Remove(entity);
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
