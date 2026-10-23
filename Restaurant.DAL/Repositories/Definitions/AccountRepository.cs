using Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace Restaurant.DAL.Repositories.Definitions;

public class AccountRepository : IAccountRepository
{
    private const string CustomerRole = "User";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountRepository(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    public async Task<ApplicationUser> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<List<string>> GetRolesAsync(ApplicationUser user)
    {
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task<ResultMessage<ApplicationUser>> RegisterCustomerAsync(ApplicationUser entity, string password)
    {
        if (!await _roleManager.RoleExistsAsync(CustomerRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(CustomerRole));
        }

        entity.EmailConfirmed = true;

        var created = await _userManager.CreateAsync(entity, password);
        if (!created.Succeeded)
        {
            return new ResultMessage<ApplicationUser>
            {
                Success = false,
                Message = "Identity." + created.Errors.First().Code
            };
        }

        await _userManager.AddToRoleAsync(entity, CustomerRole);

        return new ResultMessage<ApplicationUser>
        {
            Success = true,
            Data = entity
        };
    }

    public async Task<ResultMessage<ApplicationUser>> CheckPasswordAsync(string email, string password)
    {
        // the sign-in box accepts the e-mail or the user name
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        if (user is null)
        {
            return new ResultMessage<ApplicationUser>
            {
                Success = false,
                Message = "Auth_InvalidLogin"
            };
        }

        var check = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (check.IsLockedOut)
        {
            return new ResultMessage<ApplicationUser>
            {
                Success = false,
                Message = "Auth_LockedOut"
            };
        }

        if (!check.Succeeded)
        {
            return new ResultMessage<ApplicationUser>
            {
                Success = false,
                Message = "Auth_InvalidLogin"
            };
        }

        if (!user.EmailConfirmed && await _userManager.IsInRoleAsync(user, CustomerRole))
        {
            return new ResultMessage<ApplicationUser>
            {
                Success = false,
                Message = "Auth_ConfirmFirst",
                Data = user
            };
        }

        return new ResultMessage<ApplicationUser>
        {
            Success = true,
            Data = user
        };
    }

    public async Task<ResultMessage<AccountIdentity>> SignInAsync(string email, string password)
    {
        var checkedUser = await CheckPasswordAsync(email, password);
        if (!checkedUser.Success)
        {
            return new ResultMessage<AccountIdentity> { Success = false, Message = checkedUser.Message };
        }

        return new ResultMessage<AccountIdentity>
        {
            Success = true,
            Data = new AccountIdentity
            {
                Id = checkedUser.Data.Id,
                UserName = checkedUser.Data.UserName,
                Email = checkedUser.Data.Email,
                Image = checkedUser.Data.Image,
                Roles = await GetRolesAsync(checkedUser.Data)
            }
        };
    }

    public async Task<AccountToken> GetConfirmationTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.EmailConfirmed)
        {
            return null;
        }

        return new AccountToken
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            Token = await _userManager.GenerateEmailConfirmationTokenAsync(user)
        };
    }

    public async Task<AccountToken> GetPasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        return new AccountToken
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            Token = await _userManager.GeneratePasswordResetTokenAsync(user)
        };
    }

    public async Task<string> GenerateConfirmationTokenAsync(ApplicationUser user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<ResultMessage> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return new ResultMessage
            {
                Success = result.Succeeded,
                Message = result.Succeeded ? null : "Identity." + result.Errors.First().Code
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<ResultMessage> ResetPasswordAsync(string email, string token, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "Identity." + result.Errors.First().Code
                };
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            return new ResultMessage { Success = true };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ResultMessage> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return new ResultMessage
                {
                    Success = false,
                    Message = "RecordNotFound"
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                return new ResultMessage { Success = true };
            }

            var wrongCurrent = result.Errors.Any(e => e.Code == "PasswordMismatch");

            return new ResultMessage
            {
                Success = false,
                Message = wrongCurrent ? "Auth_WrongPassword" : "Identity." + result.Errors.First().Code
            };
        }
        catch (Exception ex)
        {
            return new ResultMessage { Success = false, Message = ErrorLog.Message(ex) };
        }
    }

    public async Task<ApplicationUser> GetByIdAsync(string userId)
    {
        return await _context.Users
            .Include(a => a.Town).ThenInclude(a => a.Governorate)
            .FirstOrDefaultAsync(a => a.Id == userId);
    }
}
