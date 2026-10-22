using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.UnitPrice;

public class UnitPriceVmValidator : AbstractValidator<UnitPriceViewModel>
{
    public UnitPriceVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.UnitPrice> validator)
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

        // optional: empty is allowed, a given value must not be negative
        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Value.HasValue)
            .WithMessage(localizer["Val.NotNegative"].Value);
    }
}
