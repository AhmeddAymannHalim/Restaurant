using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.DAL.IRepositories.Definitions;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.EmployeeAccount;

public class EmployeeAccountVmValidator : AbstractValidator<EmployeeAccountViewModel>
{
    public EmployeeAccountVmValidator(IStringLocalizer<SharedResource> localizer, IUserRepository users)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);
        string MinLength(int length) => string.Format(localizer["Val.MinLength"].Value, length);

        RuleFor(x => x.EmployeeId)
            .NotNull()
            .When(x => string.IsNullOrEmpty(x.Id))
            .WithMessage(required)
            .GreaterThan(0)
            .When(x => string.IsNullOrEmpty(x.Id))
            .WithMessage(required);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, userName) => !users.IsUserNameExist(model.Id, userName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(required)
            .EmailAddress()
            .WithMessage(localizer["Auth_EmailInvalid"].Value)
            .Must((model, email) => !users.IsEmailExist(model.Id, email))
            .WithMessage(localizer["EmailAlreadyExists"].Value);

        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.Id))
            .WithMessage(required);

        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(MinLength(6));

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage(localizer["Auth_PasswordMismatch"].Value);
    }
}
