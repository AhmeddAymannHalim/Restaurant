using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.IServices.Definitions;

public interface IGalleryService
{
    Task<PaginatedResult<GalleryViewModel>> GetPagedGalleryAsync(PageRequest request);
    Task<GalleryViewModel> GetGalleryByIdAsync(int id);
    Task<ResultMessage> UpsertGalleryAsync(GalleryViewModel model);
    Task<ResultMessage> DeleteGalleryAsync(int id);
}
