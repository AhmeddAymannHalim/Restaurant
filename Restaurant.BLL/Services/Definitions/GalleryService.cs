using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class GalleryService : IGalleryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GalleryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<GalleryViewModel>> GetPagedGalleryAsync(PageRequest request)
    {
        var result = await _unitOfWork.Galleries.GetPagedGalleryAsync(request);

        return PaginatedResult<GalleryViewModel>.Create(_mapper.Map<List<GalleryViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<GalleryViewModel> GetGalleryByIdAsync(int id)
    {
        var entity = await _unitOfWork.Galleries.GetGalleryByIdAsync(id);

        return _mapper.Map<GalleryViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertGalleryAsync(GalleryViewModel model)
    {
        var entity = _mapper.Map<GalleryItem>(model);

        return await _unitOfWork.Galleries.UpsertGalleryAsync(entity, model.ImageFile);
    }

    public async Task<ResultMessage> DeleteGalleryAsync(int id)
    {
        return await _unitOfWork.Galleries.DeleteGalleryAsync(id);
    }
}
