using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Deposit;

public class DepositVmValidator : AbstractValidator<DepositViewModel>
{
    public DepositVmValidator(IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Deposit> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);


        RuleFor(x => x.DeposerName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Description)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));
    }
}
