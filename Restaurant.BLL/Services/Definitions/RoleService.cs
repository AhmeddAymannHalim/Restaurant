using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;
    private readonly IMapper _mapper;

    public RoleService(IRoleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<RoleViewModel>> GetPagedRolesAsync(PageRequest request)
    {
        var result = await _repository.GetPagedRolesAsync(request);

        return PaginatedResult<RoleViewModel>.Create(_mapper.Map<List<RoleViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<RoleViewModel> GetRoleByIdAsync(string id)
    {
        var entity = await _repository.GetRoleByIdAsync(id);

        return _mapper.Map<RoleViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertRoleAsync(RoleViewModel model)
    {
        return await _repository.UpsertRoleAsync(_mapper.Map<IdentityRole>(model));
    }

    public async Task<ResultMessage> DeleteRoleAsync(string id)
    {
        return await _repository.DeleteRoleAsync(id);
    }
}
