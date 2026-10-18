using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.InsuranceCategory;

public class InsuranceCategoryVmValidator : AbstractValidator<InsuranceCategoryViewModel>
{
    public InsuranceCategoryVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.InsuranceCategory> validator)
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

        RuleFor(x => x.EmployeePercent)
            .InclusiveBetween(0, 100)
            .When(x => x.EmployeePercent.HasValue)
            .WithMessage(localizer["Val.PercentRange"].Value);

        RuleFor(x => x.CompanyPercent)
            .InclusiveBetween(0, 100)
            .When(x => x.CompanyPercent.HasValue)
            .WithMessage(localizer["Val.PercentRange"].Value);

        RuleFor(x => x.BasicWage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BasicWage.HasValue)
            .WithMessage(required);

        RuleFor(x => x.AllowancesAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AllowancesAmount.HasValue)
            .WithMessage(required);
    }
}
