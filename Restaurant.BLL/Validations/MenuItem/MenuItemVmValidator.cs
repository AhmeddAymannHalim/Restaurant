using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.MenuItem;

public class MenuItemVmValidator : AbstractValidator<MenuItemViewModel>
{
    public MenuItemVmValidator(IStringLocalizer<SharedResource> localizer, IValidatorService<Restaurant.DAL.Models.MenuItem> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName)
            .Must((model, value) => !validator.IsArNameExist(model.Id, value))
            .When(x => !string.IsNullOrWhiteSpace(x.ArName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .Must((model, value) => !validator.IsEnNameExist(model.Id, value))
            .When(x => !string.IsNullOrWhiteSpace(x.EnName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

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
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.StoreId)
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
