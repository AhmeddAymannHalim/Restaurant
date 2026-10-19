using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Branch;

public class BranchVmValidator : AbstractValidator<BranchViewModel>
{
    public BranchVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.CompanyBranch> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(150)
            .WithMessage(MaxLength(150))
            .Must((model, arName) => !validator.IsArNameExist(model.Id, arName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(150)
            .WithMessage(MaxLength(150))
            .Must((model, enName) => !validator.IsEnNameExist(model.Id, enName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.Phone1)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.Phone2)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.Mobile1)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));
    }
}
