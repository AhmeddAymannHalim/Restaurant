using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class ContractTypeService : IContractTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContractTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ContractTypeViewModel>> GetPagedContractTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.ContractTypes.GetPagedContractTypesAsync(request);

        return PaginatedResult<ContractTypeViewModel>.Create(_mapper.Map<List<ContractTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<ContractTypeViewModel> GetContractTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.ContractTypes.GetContractTypeByIdAsync(id);

        return _mapper.Map<ContractTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertContractTypeAsync(ContractTypeViewModel model)
    {
        var entity = _mapper.Map<ContractType>(model);

        return await _unitOfWork.ContractTypes.UpsertContractTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteContractTypeAsync(int id)
    {
        return await _unitOfWork.ContractTypes.DeleteContractTypeAsync(id);
    }
}
