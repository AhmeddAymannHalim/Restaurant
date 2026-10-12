using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<UserViewModel>> GetPagedUsersAsync(PageRequest request)
    {
        var result = await _repository.GetPagedUsersAsync(request);

        return PaginatedResult<UserViewModel>.Create(_mapper.Map<List<UserViewModel>>(result.Data), result.TotalRecords, request);
    }

    public async Task<UserViewModel> GetUserByIdAsync(string id)
    {
        var entity = await _repository.GetUserByIdAsync(id);

        return _mapper.Map<UserViewModel>(entity);
    }

    public async Task<ResultMessage> UpsertUserAsync(UserViewModel model)
    {
        var entity = _mapper.Map<ApplicationUser>(model);

        return await _repository.UpsertUserAsync(entity, model.Password);
    }

    public async Task<ResultMessage> DeleteUserAsync(string id)
    {
        return await _repository.DeleteUserAsync(id);
    }
}
