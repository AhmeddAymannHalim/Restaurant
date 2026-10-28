using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Tax;

public class TaxVmValidator : AbstractValidator<TaxViewModel>
{
    public TaxVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Tax> validator)
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

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage(required)
            .InclusiveBetween(0, 100)
            .WithMessage(localizer["Val.PercentRange"].Value);
    }
}
