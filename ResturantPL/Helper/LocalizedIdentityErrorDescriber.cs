using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Shared;

namespace ResturantPL.Helper;

// Identity messages come in English by default; the text here follows the request language.
public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = _localizer["Identity.DefaultError"].Value };
    public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = _localizer["Identity.ConcurrencyFailure"].Value };
    public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer["Identity.PasswordMismatch"].Value };
    public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = _localizer["Identity.InvalidToken"].Value };
    public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer["Identity.LoginAlreadyAssociated"].Value };
    public override IdentityError InvalidUserName(string userName) => new IdentityError { Code = nameof(InvalidUserName), Description = string.Format(_localizer["Identity.InvalidUserName"].Value, userName) };
    public override IdentityError InvalidEmail(string email) => new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(_localizer["Identity.InvalidEmail"].Value, email) };
    public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(_localizer["Identity.DuplicateUserName"].Value, userName) };
    public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = nameof(DuplicateEmail), Description = string.Format(_localizer["Identity.DuplicateEmail"].Value, email) };
    public override IdentityError InvalidRoleName(string role) => new IdentityError { Code = nameof(InvalidRoleName), Description = string.Format(_localizer["Identity.InvalidRoleName"].Value, role) };
    public override IdentityError DuplicateRoleName(string role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = string.Format(_localizer["Identity.DuplicateRoleName"].Value, role) };
    public override IdentityError UserAlreadyHasPassword() => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer["Identity.UserAlreadyHasPassword"].Value };
    public override IdentityError UserLockoutNotEnabled() => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer["Identity.UserLockoutNotEnabled"].Value };
    public override IdentityError UserAlreadyInRole(string role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(_localizer["Identity.UserAlreadyInRole"].Value, role) };
    public override IdentityError UserNotInRole(string role) => new IdentityError { Code = nameof(UserNotInRole), Description = string.Format(_localizer["Identity.UserNotInRole"].Value, role) };
    public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(_localizer["Identity.PasswordTooShort"].Value, length) };
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = string.Format(_localizer["Identity.PasswordRequiresUniqueChars"].Value, uniqueChars) };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["Identity.PasswordRequiresNonAlphanumeric"].Value };
    public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer["Identity.PasswordRequiresDigit"].Value };
    public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer["Identity.PasswordRequiresLower"].Value };
    public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer["Identity.PasswordRequiresUpper"].Value };
}
