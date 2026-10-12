using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.DAL.IRepositories.Definitions;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Account;

public class SignInVmValidator : AbstractValidator<SignInViewModel>
{
    public SignInVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Auth_EmailRequired"].Value)
            .EmailAddress()
            .WithMessage(localizer["Auth_EmailInvalid"].Value);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["Auth_PasswordRequired"].Value);
    }
}

public class RegisterVmValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterVmValidator(IStringLocalizer<SharedResource> localizer, IUserRepository users)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(localizer["Auth_FirstRequired"].Value)
            .Length(3, 10)
            .WithMessage(localizer["Auth_NameLen"].Value);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(localizer["Auth_LastRequired"].Value)
            .Length(3, 10)
            .WithMessage(localizer["Auth_NameLen"].Value);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Auth_EmailRequired"].Value)
            .EmailAddress()
            .WithMessage(localizer["Auth_EmailInvalid"].Value)
            .Must(email => !users.IsEmailExist(null, email))
            .WithMessage(localizer["EmailAlreadyExists"].Value);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(localizer["Auth_PhoneRequired"].Value)
            .Length(11)
            .WithMessage(localizer["Auth_PhoneLen"].Value);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["Auth_PasswordRequired"].Value)
            .MinimumLength(6)
            .WithMessage(localizer["Auth_PasswordMin"].Value);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage(localizer["Auth_PasswordMismatch"].Value);

        RuleFor(x => x.IsAgree)
            .Equal(true)
            .WithMessage(localizer["Auth_AgreeRequired"].Value);
    }
}

public class ForgetPasswordVmValidator : AbstractValidator<ForgetPasswordViewModel>
{
    public ForgetPasswordVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Auth_EmailRequired"].Value)
            .EmailAddress()
            .WithMessage(localizer["Auth_EmailInvalid"].Value);
    }
}

public class ResetPasswordVmValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(localizer["Auth_PasswordRequired"].Value)
            .MinimumLength(6)
            .WithMessage(localizer["Auth_PasswordMin"].Value);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage(localizer["Auth_PasswordMismatch"].Value);
    }
}

public class ChangePasswordVmValidator : AbstractValidator<ChangePasswordViewModel>
{
    public ChangePasswordVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage(localizer["Auth_PasswordRequired"].Value);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(localizer["Auth_PasswordRequired"].Value)
            .MinimumLength(6)
            .WithMessage(localizer["Auth_PasswordMin"].Value)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage(localizer["Auth_SamePassword"].Value);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage(localizer["Auth_PasswordMismatch"].Value);
    }
}
