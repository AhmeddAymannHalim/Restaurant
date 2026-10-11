using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class SalaryTypeService : ISalaryTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalaryTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SalaryTypeViewModel>> GetPagedSalaryTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.SalaryTypes.GetPagedSalaryTypesAsync(request);

        return PaginatedResult<SalaryTypeViewModel>.Create(_mapper.Map<List<SalaryTypeViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<SalaryTypeViewModel> GetSalaryTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.SalaryTypes.GetSalaryTypeByIdAsync(id);

        return _mapper.Map<SalaryTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertSalaryTypeAsync(SalaryTypeViewModel model)
    {
        var entity = _mapper.Map<SalaryType>(model);

        return await _unitOfWork.SalaryTypes.UpsertSalaryTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteSalaryTypeAsync(int id)
    {
        return await _unitOfWork.SalaryTypes.DeleteSalaryTypeAsync(id);
    }
}
