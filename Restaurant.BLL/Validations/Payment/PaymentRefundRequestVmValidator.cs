using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Payment;

public class PaymentRefundRequestVmValidator : AbstractValidator<PaymentRefundRequestViewModel>
{
    public PaymentRefundRequestVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.OriginalPaymentId)
            .GreaterThan(0)
            .WithMessage(localizer["Val.RequiredField"].Value);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(localizer["Val.GreaterThanZero"].Value);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(250)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 250));
    }
}
