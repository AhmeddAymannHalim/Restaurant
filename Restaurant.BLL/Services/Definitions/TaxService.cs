using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class TaxService : ITaxService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TaxService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TaxViewModel>> GetPagedTaxesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Taxes.GetPagedTaxesAsync(request);

        return PaginatedResult<TaxViewModel>.Create(_mapper.Map<List<TaxViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<TaxViewModel> GetTaxByIdAsync(int id)
    {
        var entity = await _unitOfWork.Taxes.GetTaxByIdAsync(id);

        return _mapper.Map<TaxViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertTaxAsync(TaxViewModel model)
    {
        var entity = _mapper.Map<Tax>(model);

        return await _unitOfWork.Taxes.UpsertTaxAsync(entity);
    }

    public async Task<ResultMessage> DeleteTaxAsync(int id)
    {
        return await _unitOfWork.Taxes.DeleteTaxAsync(id);
    }
}
