using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Chef;

public class ChefVmValidator : AbstractValidator<ChefViewModel>
{
    public ChefVmValidator(IStringLocalizer<SharedResource> localizer, IValidatorService<Restaurant.DAL.Models.ChefItem> validator)
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

        RuleFor(x => x.ArName).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.EnName).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.Jop).NotEmpty().WithMessage(required).MaximumLength(100).WithMessage(MaxLength(100));
        RuleFor(x => x.ImageFile).NotNull().When(x => x.Id == 0).WithMessage(required);
    }
}
