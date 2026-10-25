using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Store;

public class StoreVmValidator : AbstractValidator<StoreViewModel>
{
    public StoreVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Store> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        this.AddPhoneNumberRules(localizer, x => x.PhoneNumber, x => x.PhoneNumber2, x => x.PhoneNumber3);

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
            .Must((model, number) => !validator.IsValueExist(model.Id, "PhoneNumber1", number))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.Code)
            .Must((model, code) => !validator.IsCodeExist(model.Id, code))
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(localizer["Val.CodeAlreadyExist"].Value);

        RuleFor(x => x.ArName_Abbr)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.EnName_Abbr)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.Printer)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.ParentId)
            .NotEqual(x => x.Id)
            .When(x => x.ParentId.HasValue && x.Id > 0)
            .WithMessage(localizer["Val.CannotBeOwnParent"].Value);
    }
}
