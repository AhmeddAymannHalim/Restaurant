using AutoMapper;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Services.Definitions;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResultMessage<AccountToken>> RegisterAsync(RegisterViewModel model)
    {
        var created = await _repository.RegisterCustomerAsync(_mapper.Map<ApplicationUser>(model), model.Password);

        return _mapper.Map<ResultMessage<AccountToken>>(created);
    }

    public async Task<ResultMessage<AccountIdentity>> SignInAsync(SignInViewModel model)
    {
        return await _repository.SignInAsync(model.Email, model.Password);
    }

    public async Task<AccountToken> GetConfirmationTokenAsync(string email)
    {
        return await _repository.GetConfirmationTokenAsync(email);
    }

    public async Task<ResultMessage> ConfirmEmailAsync(string userId, string token)
    {
        return await _repository.ConfirmEmailAsync(userId, token);
    }

    public async Task<AccountToken> GetPasswordResetTokenAsync(string email)
    {
        return await _repository.GetPasswordResetTokenAsync(email);
    }

    public async Task<ResultMessage> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        return await _repository.ResetPasswordAsync(model.Email, model.Token, model.Password);
    }

    public async Task<ResultMessage> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
        return await _repository.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
    }

    public async Task<ProfileViewModel> GetProfileAsync(string userId)
    {
        return _mapper.Map<ProfileViewModel>(await _repository.GetByIdAsync(userId));
    }
}
