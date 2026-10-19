using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.StoreConvert;

public class StoreConvertVmValidator : AbstractValidator<StoreConvertViewModel>
{
    public StoreConvertVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FromStoreId)
            .NotNull()
            .WithMessage(localizer["Val.RequiredField"].Value);

        RuleFor(x => x.ToStoreId)
            .NotNull()
            .WithMessage(localizer["Val.RequiredField"].Value)
            .NotEqual(x => x.FromStoreId)
            .WithMessage(localizer["SameStore"].Value);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(localizer["AtLeastOneItem"].Value);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage(localizer["Val.RequiredField"].Value);
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage(localizer["Val.GreaterThanZero"].Value);
        });
    }
}
