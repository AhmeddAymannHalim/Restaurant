using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Currency;

public class CurrencyVmValidator : AbstractValidator<CurrencyViewModel>
{
    public CurrencyVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Currency> validator)
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

        RuleFor(x => x.Code)
            .MaximumLength(20)
            .WithMessage(MaxLength(20))
            .Must((model, code) => !validator.IsCodeExist(model.Id, code))
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(localizer["Val.CodeAlreadyExist"].Value);

        RuleFor(x => x.SubArName)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.SubEnName)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.CurrencyRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CurrencyRate.HasValue)
            .WithMessage(required);

        RuleFor(x => x.SubToCurrencyTrans)
            .GreaterThanOrEqualTo((short)0)
            .When(x => x.SubToCurrencyTrans.HasValue)
            .WithMessage(required);

        RuleFor(x => x.CurrencyFixedRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CurrencyFixedRate.HasValue)
            .WithMessage(required);
    }
}
