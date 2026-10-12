using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Helpers;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.Repositories.Definitions;

public class GalleryRepository : IGalleryRepository
{
    private const string ImagesFolder = "Images";

    private readonly ApplicationDbContext _context;

    public GalleryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<GalleryItem>> GetPagedGalleryAsync(PageRequest request)
    {
        var query = _context.GalleryItem.AsNoTracking().AsQueryable();

        var total = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Id)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return PaginatedResult<GalleryItem>.Create(data, total, request);
    }

    public async Task<GalleryItem> GetGalleryByIdAsync(int id)
    {
        return await _context.GalleryItem.FindAsync(id);
    }

    public async Task<ResultMessage> UpsertGalleryAsync(GalleryItem entity, IFormFile imageFile)
    {
        try
        {
            var existing = entity.Id == 0
                ? null
                : await _context.GalleryItem.AsNoTracking().FirstOrDefaultAsync(a => a.Id == entity.Id);
            if (entity.Id != 0 && existing is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            entity.PictureUrl = existing?.PictureUrl;
            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existing?.PictureUrl))
                {
                    DocumentSetting.DeleteFile(existing.PictureUrl);
                }

                entity.PictureUrl = DocumentSetting.UploadFile(imageFile, ImagesFolder);
            }

            if (entity.Id == 0)
            {
                await _context.GalleryItem.AddAsync(entity);
            }
            else
            {
                _context.GalleryItem.Update(entity);
            }

            await _context.SaveChangesAsync();

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

    public async Task<ResultMessage> DeleteGalleryAsync(int id)
    {
        try
        {
            var entity = await _context.GalleryItem.FindAsync(id);
            if (entity is null)
            {
                return new ResultMessage { Success = true };
            }

            _context.GalleryItem.Remove(entity);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(entity.PictureUrl))
            {
                DocumentSetting.DeleteFile(entity.PictureUrl);
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
