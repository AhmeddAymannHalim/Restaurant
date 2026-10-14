using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Employee;

public class EmployeeVmValidator : AbstractValidator<EmployeeViewModel>
{
    public EmployeeVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Employee> validator)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);
        string MinLength(int length) => string.Format(localizer["Val.MinLength"].Value, length);
        string ExactLength(int length) => string.Format(localizer["Val.ExactLength"].Value, length);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(50)
            .WithMessage(MaxLength(50))
            .Must((model, lastName) => !validator.IsFullNameExist(model.Id, model.FirstName, lastName))
            .WithMessage(localizer["EmployeeAlreadyExists"].Value);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(required)
            .Length(11)
            .WithMessage(ExactLength(11))
            .Must((model, number) => !validator.IsPhoneNumberExist(model.Id, number))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.NationalID)
            .NotEmpty()
            .WithMessage(required)
            .Length(14)
            .WithMessage(ExactLength(14))
            .Must((model, nationalId) => !validator.IsValueExist(model.Id, "NationalID", nationalId))
            .WithMessage(localizer["NationalIdAlreadyExists"].Value);

        RuleFor(x => x.Age)
            .GreaterThan((byte)0)
            .WithMessage(required);

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0)
            .WithMessage(required);

        RuleFor(x => x.SalaryTypeId)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.BrancheId)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage(required);
    }
}
