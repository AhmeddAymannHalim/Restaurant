using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.OnlineOrder;

public class OnlineOrderVmValidator : AbstractValidator<OnlineOrderCreateViewModel>
{
    public OnlineOrderVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(100).WithMessage(string.Format(localizer["Val.MaxLength"].Value, 100));

        RuleFor(x => x.Address)
            .NotEmpty()
            .When(x => !x.TableId.HasValue)
            .WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(250).WithMessage(string.Format(localizer["Val.MaxLength"].Value, 250));

        RuleFor(x => x.PhoneNumber).MaximumLength(11).WithMessage(string.Format(localizer["Val.MaxLength"].Value, 11));
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(11).WithMessage(string.Format(localizer["Val.MaxLength"].Value, 11));

        RuleFor(x => x.OnlineOrderMenuItems)
            .Must(items => items.Any(i => i.Quantity > 0))
            .WithMessage(localizer["AtLeastOneItem"].Value);
    }
}
