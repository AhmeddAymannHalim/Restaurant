using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.MenuItem;

public class MenuItemVmValidator : AbstractValidator<MenuItemViewModel>
{
    public MenuItemVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        // the same name in another size is allowed, so the repository checks name + size (MenuItemSizeExists)

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(40)
            .WithMessage(MaxLength(40));

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(40)
            .WithMessage(MaxLength(40));

        RuleFor(x => x.MenuSectionId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.StoreId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.ArDescription)
            .MaximumLength(150)
            .WithMessage(MaxLength(150));

        RuleFor(x => x.EnDescription)
            .MaximumLength(150)
            .WithMessage(MaxLength(150));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage(required);
    }
}
