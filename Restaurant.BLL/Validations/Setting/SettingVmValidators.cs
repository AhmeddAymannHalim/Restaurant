using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Setting;

public class ApplicationSettingVmValidator : AbstractValidator<ApplicationSettingViewModel>
{
    public ApplicationSettingVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.Password)
            .MaximumLength(50)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 50));

        RuleFor(x => x.OrderTax)
            .InclusiveBetween(0, 100)
            .WithMessage(localizer["Val.Between0And100"].Value);

        RuleFor(x => x.OrderDiscount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value);

        RuleFor(x => x.OrderService)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value);
    }
}

public class RestaurantSettingVmValidator : AbstractValidator<RestaurantSettingViewModel>
{
    public RestaurantSettingVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Auth_EmailInvalid"].Value);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage(MaxLength(20));
    }
}

public class PrinterSettingVmValidator : AbstractValidator<PrinterSettingViewModel>
{
    public PrinterSettingVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.PrinterForAllSections).NotEmpty().WithMessage(required);
        RuleFor(x => x.PrinterForCashier).NotEmpty().WithMessage(required);
        RuleFor(x => x.PrinterForEachSection).NotEmpty().WithMessage(required);
    }
}
