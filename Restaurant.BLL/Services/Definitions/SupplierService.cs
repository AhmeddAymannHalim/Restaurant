using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SupplierViewModel>> GetPagedSuppliersAsync(PageRequest request)
    {
        var result = await _unitOfWork.Suppliers.GetPagedSuppliersAsync(request);

        return PaginatedResult<SupplierViewModel>.Create(_mapper.Map<List<SupplierViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<SupplierViewModel> GetSupplierByIdAsync(int id)
    {
        var entity = await _unitOfWork.Suppliers.GetSupplierByIdAsync(id);

        return _mapper.Map<SupplierViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertSupplierAsync(SupplierViewModel model)
    {
        var entity = _mapper.Map<Supplier>(model);

        return await _unitOfWork.Suppliers.UpsertSupplierAsync(entity);
    }

    public async Task<ResultMessage> DeleteSupplierAsync(int id)
    {
        return await _unitOfWork.Suppliers.DeleteSupplierAsync(id);
    }
}
