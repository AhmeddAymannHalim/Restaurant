using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class AllowanceTypeService : IAllowanceTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AllowanceTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AllowanceTypeViewModel>> GetPagedAllowanceTypesAsync(PageRequest request)
    {
        var result = await _unitOfWork.AllowanceTypes.GetPagedAllowanceTypesAsync(request);

        return PaginatedResult<AllowanceTypeViewModel>.Create(_mapper.Map<List<AllowanceTypeViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<AllowanceTypeViewModel> GetAllowanceTypeByIdAsync(int id)
    {
        var entity = await _unitOfWork.AllowanceTypes.GetAllowanceTypeByIdAsync(id);

        return _mapper.Map<AllowanceTypeViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertAllowanceTypeAsync(AllowanceTypeViewModel model)
    {
        var entity = _mapper.Map<AllowanceType>(model);

        return await _unitOfWork.AllowanceTypes.UpsertAllowanceTypeAsync(entity);
    }

    public async Task<ResultMessage> DeleteAllowanceTypeAsync(int id)
    {
        return await _unitOfWork.AllowanceTypes.DeleteAllowanceTypeAsync(id);
    }
}
