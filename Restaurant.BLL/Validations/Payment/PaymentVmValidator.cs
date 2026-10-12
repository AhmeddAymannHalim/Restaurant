using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Payment;

public class PaymentVmValidator : AbstractValidator<PaymentViewModel>
{
    public PaymentVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.PartnerId)
            .GreaterThan(0)
            .WithMessage(localizer["Val.RequiredField"].Value);

        RuleFor(x => x.Paid)
            .GreaterThan(0)
            .WithMessage(localizer["Val.GreaterThanZero"].Value);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 250));
    }
}
