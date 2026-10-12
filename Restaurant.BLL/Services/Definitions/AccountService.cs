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
        if (!created.Success)
        {
            return new ResultMessage<AccountToken>
            {
                Success = false,
                Message = created.Message
            };
        }

        var token = await GetConfirmationTokenAsync(created.Data.Email);

        return new ResultMessage<AccountToken>
        {
            Success = true,
            Data = token
        };
    }

    public async Task<ResultMessage<AccountIdentity>> SignInAsync(SignInViewModel model)
    {
        var result = await _repository.CheckPasswordAsync(model.Email, model.Password);
        if (!result.Success)
        {
            return new ResultMessage<AccountIdentity>
            {
                Success = false,
                Message = result.Message
            };
        }

        var identity = new AccountIdentity
        {
            Id = result.Data.Id,
            UserName = result.Data.UserName,
            Email = result.Data.Email,
            Image = result.Data.Image,
            Roles = await _repository.GetRolesAsync(result.Data)
        };

        return new ResultMessage<AccountIdentity>
        {
            Success = true,
            Data = identity
        };
    }

    public async Task<AccountToken> GetConfirmationTokenAsync(string email)
    {
        var user = await _repository.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
        {
            return null;
        }

        return new AccountToken
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            Token = await _repository.GenerateConfirmationTokenAsync(user)
        };
    }

    public async Task<ResultMessage> ConfirmEmailAsync(string userId, string token)
    {
        return await _repository.ConfirmEmailAsync(userId, token);
    }

    public async Task<AccountToken> GetPasswordResetTokenAsync(string email)
    {
        var user = await _repository.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        return new AccountToken
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            Token = await _repository.GeneratePasswordResetTokenAsync(user)
        };
    }

    public async Task<ResultMessage> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        return await _repository.ResetPasswordAsync(model.Email, model.Token, model.Password);
    }

    public async Task<ResultMessage> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
        return await _repository.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
    }
}
