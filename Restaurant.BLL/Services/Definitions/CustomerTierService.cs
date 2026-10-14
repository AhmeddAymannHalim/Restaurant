using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CustomerTierService : ICustomerTierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerTierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CustomerTierViewModel>> GetPagedCustomerTiersAsync(PageRequest request)
    {
        var result = await _unitOfWork.CustomerTiers.GetPagedCustomerTiersAsync(request);

        return PaginatedResult<CustomerTierViewModel>.Create(_mapper.Map<List<CustomerTierViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<CustomerTierViewModel> GetCustomerTierByIdAsync(int id)
    {
        var entity = await _unitOfWork.CustomerTiers.GetCustomerTierByIdAsync(id);

        return _mapper.Map<CustomerTierViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertCustomerTierAsync(CustomerTierViewModel model)
    {
        var entity = _mapper.Map<CustomerTier>(model);

        return await _unitOfWork.CustomerTiers.UpsertCustomerTierAsync(entity);
    }

    public async Task<ResultMessage> DeleteCustomerTierAsync(int id)
    {
        return await _unitOfWork.CustomerTiers.DeleteCustomerTierAsync(id);
    }
}
