using Microsoft.AspNetCore.Http;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;

namespace Restaurant.DAL.IRepositories.Definitions;

public interface IGalleryRepository
{
    Task<PaginatedResult<GalleryItem>> GetPagedGalleryAsync(PageRequest request);
    Task<GalleryItem> GetGalleryByIdAsync(int id);
    Task<ResultMessage> UpsertGalleryAsync(GalleryItem entity, IFormFile imageFile);
    Task<ResultMessage> DeleteGalleryAsync(int id);
}
