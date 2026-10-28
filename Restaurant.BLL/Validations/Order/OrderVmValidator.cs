using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Order;

public class OrderVmValidator : AbstractValidator<OrderViewModel>
{
    public OrderVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(localizer["AtLeastOneItem"].Value);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId).GreaterThan(0).WithMessage(localizer["Val.RequiredField"].Value);
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage(localizer["Val.GreaterThanZero"].Value);
        });

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.GreaterThanZero"].Value);

        RuleFor(x => x.PointsRedeemed)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["PointsNegative"].Value);

        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Amount).GreaterThanOrEqualTo(0).WithMessage(localizer["PaymentNegative"].Value);
        });

        RuleFor(x => x.CustomerId)
            .NotNull()
            .WithMessage(localizer["CustomerRequired"].Value);

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 100));

        RuleFor(x => x.TableId)
            .NotNull()
            .When(x => x.Type == OrderType.Hall)
            .WithMessage(localizer["TableRequired"].Value);

        RuleFor(x => x.EmployeeId)
            .NotNull()
            .When(x => x.Type is OrderType.Delivery)
            .WithMessage(localizer["DeliveryDriverRequired"].Value);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .When(x => x.Type is OrderType.Delivery)
            .WithMessage(localizer["Val.RequiredField"].Value);

        RuleFor(x => x.Address)
            .NotEmpty()
            .When(x => x.Type is OrderType.Delivery)
            .WithMessage(localizer["Val.RequiredField"].Value)
            .MaximumLength(250)
            .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 250));

        RuleFor(x => x.PhoneNumber).MaximumLength(11).WithMessage(string.Format(localizer["Val.MaxLength"].Value, 11));
    }
}
