using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Store;

public class StoreVmValidator : AbstractValidator<StoreViewModel>
{
    public StoreVmValidator(IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Store> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, arName) => !validator.IsArNameExist(model.Id, arName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, enName) => !validator.IsEnNameExist(model.Id, enName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Address)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.PhoneNumber)
            .Must((model, number) => !validator.IsPhoneNumberExist(model.Id, number))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.MobileNumber)
            .MaximumLength(11)
            .WithMessage(MaxLength(11));

        RuleFor(x => x.MobileNumber)
            .Must((model, number) => !validator.IsPhoneNumberExist(model.Id, number))
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);
    }
}
