using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.DAL.IRepositories.Definitions;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.User;

public class UserVmValidator : AbstractValidator<UserViewModel>
{
    public UserVmValidator(IStringLocalizer<SharedResource> localizer, IUserRepository repository)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);
        string MinLength(int length) => string.Format(localizer["Val.MinLength"].Value, length);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, userName) => !repository.IsUserNameExist(model.Id, userName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(required)
            .EmailAddress()
            .WithMessage(localizer["Auth_EmailInvalid"].Value)
            .Must((model, email) => !repository.IsEmailExist(model.Id, email))
            .WithMessage(localizer["EmailAlreadyExists"].Value);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.UserType)
            .NotEmpty()
            .WithMessage(required);

        When(x => string.IsNullOrEmpty(x.Id), () =>
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(required)
                .MinimumLength(6)
                .WithMessage(MinLength(6));

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage(localizer["Auth_PasswordMismatch"].Value);
        });
    }
}
