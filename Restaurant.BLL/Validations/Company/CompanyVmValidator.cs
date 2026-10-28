using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Company;

public class CompanyVmValidator : AbstractValidator<CompanyViewModel>
{
    public CompanyVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Company> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, arName) => !validator.IsArNameExist(model.Id, arName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, enName) => !validator.IsEnNameExist(model.Id, enName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Address)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.Phone1)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.Phone2)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Val.InvalidEmail"].Value)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.VatNumber)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));
    }
}
