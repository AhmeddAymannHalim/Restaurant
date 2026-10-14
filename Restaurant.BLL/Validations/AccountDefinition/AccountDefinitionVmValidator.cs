using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.AccountDefinition;

public class AccountDefinitionVmValidator : AbstractValidator<AccountDefinitionViewModel>
{
    public AccountDefinitionVmValidator(IStringLocalizer<SharedResource> localizer, IValidatorService<Restaurant.DAL.Models.Account> validator)
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

        RuleFor(x => x.Code)
            .Must((model, code) => !validator.IsCodeExist(model.Id, code))
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(localizer["Val.CodeAlreadyExist"].Value);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(150)
            .WithMessage(MaxLength(150));

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(150)
            .WithMessage(MaxLength(150));

        RuleFor(x => x.Kind)
            .Must(kind => kind != AccountKind.Owned)
            .WithMessage(localizer["AccountOwnedByAnotherScreen"].Value);

        RuleFor(x => x.Remarks)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.ParentAccountId)
            .NotEqual(x => x.Id)
            .When(x => x.ParentAccountId.HasValue && x.Id > 0)
            .WithMessage(localizer["InvalidParent"].Value);

        RuleFor(x => x.MaxDebitAllowed)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxDebitAllowed.HasValue)
            .WithMessage(required);

        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage(required);

        When(x => x.Kind == AccountKind.Asset, () =>
        {
            RuleFor(x => x.Location)
                .MaximumLength(200)
                .WithMessage(MaxLength(200));

            RuleFor(x => x.BuyValue)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BuyValue.HasValue)
                .WithMessage(required);

            RuleFor(x => x.ScrapValue)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ScrapValue.HasValue)
                .WithMessage(required)
                .LessThanOrEqualTo(x => x.BuyValue ?? decimal.MaxValue)
                .When(x => x.ScrapValue.HasValue && x.BuyValue.HasValue)
                .WithMessage(localizer["Asset.ScrapAboveBuy"].Value);

            RuleFor(x => x.ProductAge)
                .Must(age => age.HasValue && age.Value > 0)
                .When(x => x.DepreciationType != DepreciationType.None)
                .WithMessage(localizer["Asset.AgeRequired"].Value);

            RuleFor(x => x.StartUsingDate)
                .GreaterThanOrEqualTo(x => x.BuyDate.Value)
                .When(x => x.StartUsingDate.HasValue && x.BuyDate.HasValue)
                .WithMessage(localizer["Asset.UseBeforeBuy"].Value);
        });
    }
}
