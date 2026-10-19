using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Invoice;

public class InvoiceVmValidator : AbstractValidator<InvoiceViewModel>
{
    public InvoiceVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.SupplierId)
            .Must(id => id.GetValueOrDefault() > 0)
            .WithMessage(required)
            .When(x => x.HasPartner && x.IsSupplierInvoice);

        RuleFor(x => x.CustomerId)
            .Must(id => id.GetValueOrDefault() > 0)
            .WithMessage(required)
            .When(x => x.HasPartner && !x.IsSupplierInvoice);

        RuleFor(x => x.StoreId)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(localizer["InvoiceNeedsItems"].Value);

        RuleForEach(x => x.Items).SetValidator(new InvoiceItemVmValidator(localizer));

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value)
            .LessThanOrEqualTo(x => x.SubTotal)
            .WithMessage(localizer["Val.DiscountTooBig"].Value);

        RuleFor(x => x.Paid)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value)
            .Must((x, paid) => paid + x.LinkedPayments <= x.TotalPrice)
            .WithMessage(localizer["Val.PaidTooBig"].Value);
    }
}

public class InvoiceItemVmValidator : AbstractValidator<InvoiceItemViewModel>
{
    public InvoiceItemVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage(required);
        RuleFor(x => x.UnitPriceId).GreaterThan(0).WithMessage(required);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage(localizer["Val.GreaterThanZero"].Value);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage(localizer["Val.NotNegative"].Value);
    }
}
