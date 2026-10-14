using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Supplier;

public class SupplierVmValidator : AbstractValidator<SupplierViewModel>
{
    public SupplierVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Account> accountValidator,
        IValidatorService<Restaurant.DAL.Models.Supplier> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.Email)
            .Must((model, value) => !validator.IsValueExist(model.Id, "Email", value))
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Val.AlreadyExist"].Value);

        RuleFor(x => x.VatNumber)
            .Must((model, value) => !validator.IsValueExist(model.Id, "VatNumber", value))
            .When(x => !string.IsNullOrWhiteSpace(x.VatNumber))
            .WithMessage(localizer["Val.AlreadyExist"].Value);

        this.AddPhoneRules(x => x.PhoneNumber, x => x.Phones, localizer);

        RuleFor(x => x.Code)
            .Must((model, code) => !accountValidator.IsCodeExist(model.AccountId ?? 0, code))
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(localizer["Val.CodeAlreadyExist"].Value);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, arName) => !accountValidator.IsArNameExist(model.AccountId ?? 0, arName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, enName) => !accountValidator.IsEnNameExist(model.AccountId ?? 0, enName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Address)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.PhoneNumber)
            .Must((model, number) => !validator.IsValueExist(model.Id, "PhoneNumber1", number))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Val.InvalidEmail"].Value)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .WithMessage(MaxLength(200));

        RuleFor(x => x.StreetName)
            .MaximumLength(150)
            .WithMessage(MaxLength(150));

        RuleFor(x => x.BuildingNo)
            .MaximumLength(30)
            .WithMessage(MaxLength(30));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage(MaxLength(20));

        RuleFor(x => x.VatNumber)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.Remark)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CreditLimit.HasValue)
            .WithMessage(required);

        RuleFor(x => x.CreditDurationInDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CreditDurationInDays.HasValue)
            .WithMessage(required);
    }
}
