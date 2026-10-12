using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class ChefRepository : IChefRepository
{
    private const string ImagesFolder = "Images";

    private readonly ApplicationDbContext _context;

    public ChefRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<ChefItem>> GetPagedChefsAsync(PageRequest request)
    {
        var query = _context.ChefItem.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var search = request.SearchString.Trim().ToLower();
            query = query.Where(a => a.ArName.ToLower().Contains(search)
                || a.EnName.ToLower().Contains(search)
                || a.Jop.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<ChefItem>.Create(data, total, request);
    }

    public async Task<ChefItem> GetChefByIdAsync(int id)
    {
        return await _context.ChefItem.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertChefAsync(ChefItem entity, IFormFile imageFile)
    {
        try
        {
            if (entity.Id == 0)
            {
                if (imageFile != null)
                {
                    entity.ImageUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
                }

                await _context.ChefItem.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.ChefItem.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
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

                _context.ChefItem.Update(entity);
                await _context.SaveChangesAsync();
            }

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ResultMessage> DeleteChefAsync(int id)
    {
        try
        {
            var entity = await _context.ChefItem.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.ChefItem.Remove(entity);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                DocumentSetting.DeleteFile(entity.ImageUrl);
            }

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
                Message = ex.Message
            };
        }
    }
}
