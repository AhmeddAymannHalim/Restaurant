using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Chef;

public class ChefVmValidator : AbstractValidator<ChefViewModel>
{
    public ChefVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.EnName).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.Jop).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.ImageFile).NotNull().When(x => x.Id == 0).WithMessage(required);
    }
}
