using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductViewModel>> GetPagedProductsAsync(PageRequest request)
    {
        var result = await _unitOfWork.Products.GetPagedProductsAsync(request);

        return PaginatedResult<ProductViewModel>.Create(_mapper.Map<List<ProductViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ProductViewModel> GetProductByIdAsync(int id)
    {
        var entity = await _unitOfWork.Products.GetProductByIdAsync(id);

        return _mapper.Map<ProductViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertProductAsync(ProductViewModel model)
    {
        var entity = _mapper.Map<Product>(model);

        var stocks = model.Stocks.Select(a => new StoreProduct
        {
            StoreId = a.StoreId,
            Quantity = a.Quantity
        }).ToList();

        return await _unitOfWork.Products.UpsertProductAsync(entity, stocks);
    }

    public async Task<ResultMessage> DeleteProductAsync(int id)
    {
        return await _unitOfWork.Products.DeleteProductAsync(id);
    }
}
