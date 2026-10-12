using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Scrounge;

public class ScroungeVmValidator : AbstractValidator<ScroungeViewModel>
{
    public ScroungeVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Scrounge> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Info)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));
    }
}
