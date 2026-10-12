using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class TableService : ITableService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TableService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TableViewModel>> GetPagedTablesAsync(PageRequest request)
    {
        var result = await _unitOfWork.Tables.GetPagedTablesAsync(request);

        return PaginatedResult<TableViewModel>.Create(_mapper.Map<List<TableViewModel>>(result.Data),
            result.TotalRecords,
            request);
    }

    public async Task<TableViewModel> GetTableByIdAsync(int id)
    {
        var entity = await _unitOfWork.Tables.GetTableByIdAsync(id);

        return _mapper.Map<TableViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertTableAsync(TableViewModel model)
    {
        var entity = _mapper.Map<Restaurant.DAL.Models.Table>(model);

        return await _unitOfWork.Tables.UpsertTableAsync(entity);
    }

    public async Task<ResultMessage> DeleteTableAsync(int id)
    {
        return await _unitOfWork.Tables.DeleteTableAsync(id);
    }
}
