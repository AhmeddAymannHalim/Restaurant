using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductCategoryViewModel>> GetPagedProductCategoriesAsync(PageRequest request)
    {
        var result = await _unitOfWork.ProductCategories.GetPagedProductCategoriesAsync(request);

        return PaginatedResult<ProductCategoryViewModel>.Create(_mapper.Map<List<ProductCategoryViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ProductCategoryViewModel> GetProductCategoryByIdAsync(int id)
    {
        var entity = await _unitOfWork.ProductCategories.GetProductCategoryByIdAsync(id);

        return _mapper.Map<ProductCategoryViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertProductCategoryAsync(ProductCategoryViewModel model)
    {
        var entity = _mapper.Map<ProductCategory>(model);

        return await _unitOfWork.ProductCategories.UpsertProductCategoryAsync(entity);
    }

    public async Task<ResultMessage> DeleteProductCategoryAsync(int id)
    {
        return await _unitOfWork.ProductCategories.DeleteProductCategoryAsync(id);
    }
}
