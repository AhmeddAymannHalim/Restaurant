using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CustomerViewModel>> GetPagedCustomersAsync(PageRequest request)
    {
        var result = await _unitOfWork.Customers.GetPagedCustomersAsync(request);

        return PaginatedResult<CustomerViewModel>.Create(_mapper.Map<List<CustomerViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<CustomerViewModel> GetCustomerByIdAsync(int id)
    {
        var entity = await _unitOfWork.Customers.GetCustomerByIdAsync(id);

        return _mapper.Map<CustomerViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertCustomerAsync(CustomerViewModel model)
    {
        var entity = _mapper.Map<Customer>(model);

        return await _unitOfWork.Customers.UpsertCustomerAsync(entity, model.ImageFile);
    }

    public async Task<ResultMessage> DeleteCustomerAsync(int id)
    {
        return await _unitOfWork.Customers.DeleteCustomerAsync(id);
    }

    public async Task<decimal?> GetCustomerBalanceAsync(int id)
    {
        return await _unitOfWork.Customers.GetCustomerBalanceAsync(id);
    }
}
