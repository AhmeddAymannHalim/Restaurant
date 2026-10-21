using System.Linq.Expressions;
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

        RuleFor(x => x.Email)
            .Must((model, value) => !validator.IsValueExist(model.Id, "Email", value))
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Val.AlreadyExist"].Value);
        string MinLength(int length) => string.Format(localizer["Val.MinLength"].Value, length);
        string ExactLength(int length) => string.Format(localizer["Val.ExactLength"].Value, length);

        RuleFor(x => x.ArName)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, arName) => !validator.IsArNameExist(model.Id, arName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.EnName)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(100)
            .WithMessage(MaxLength(100))
            .Must((model, enName) => !validator.IsEnNameExist(model.Id, enName))
            .WithMessage(localizer["Val.NameAlreadyExist"].Value);

        RuleFor(x => x.Code)
            .Must((model, code) => !validator.IsCodeExist(model.Id, code))
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(localizer["Val.CodeAlreadyExist"].Value);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(required)
            .Length(11)
            .WithMessage(ExactLength(11))
            .Must((model, number) => !validator.IsPhoneNumberExist(model.Id, number))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.TownId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage(required)
            .MinimumLength(3)
            .WithMessage(MinLength(3))
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.IDNumber)
            .NotEmpty()
            .WithMessage(required)
            .Length(14)
            .WithMessage(ExactLength(14))
            .Must((model, idNumber) => !validator.IsValueExist(model.Id, "IDNumber", idNumber))
            .WithMessage(localizer["NationalIdAlreadyExists"].Value);

        RuleFor(x => x.BasicSalary)
            .NotNull()
            .WithMessage(required)
            .GreaterThanOrEqualTo(0)
            .WithMessage(required);

        RuleForEach(x => x.Phones)
            .ChildRules(phone =>
            {
                phone.RuleFor(a => a.PhoneNumber)
                    .MaximumLength(20)
                    .WithMessage(MaxLength(20))
                    .Matches(@"^[0-9+\-\s]*$")
                    .WithMessage(localizer["Val.PhoneDigitsOnly"].Value);
            });

        RuleFor(x => x.Phones)
            .Must(phones => phones
                .Select(a => a.PhoneNumber?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .GroupBy(a => a)
                .All(a => a.Count() == 1))
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x)
            .Must(model => !model.Phones.Any(a => !string.IsNullOrWhiteSpace(a.PhoneNumber) && a.PhoneNumber.Trim() == model.PhoneNumber?.Trim()))
            .WithName("Phones")
            .WithMessage(localizer["PhoneAlreadyExists"].Value);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(localizer["Val.InvalidEmail"].Value)
            .MaximumLength(100)
            .WithMessage(MaxLength(100));

        RuleFor(x => x.IBAN)
            .MaximumLength(34)
            .WithMessage(MaxLength(34));

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50)
            .WithMessage(MaxLength(50));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        RuleFor(x => x.SalaryNotes)
            .MaximumLength(500)
            .WithMessage(MaxLength(500));

        foreach (var percent in new Expression<Func<EmployeeViewModel, decimal?>>[]
                 {
                     x => x.EmployeeInsurancePercent, x => x.CompanyInsurancePercent
                 })
        {
            RuleFor(percent)
                .InclusiveBetween(0, 100)
                .When(x => true)
                .WithMessage(localizer["Val.PercentRange"].Value);
        }

        RuleFor(x => x.NationalIdExpiryDate)
            .GreaterThanOrEqualTo(x => x.NationalIdIssueDate)
            .When(x => x.NationalIdExpiryDate.HasValue && x.NationalIdIssueDate.HasValue)
            .WithMessage(localizer["Val.ExpiryBeforeIssue"].Value);

        RuleFor(x => x.ContractEndDate)
            .GreaterThanOrEqualTo(x => x.ContractDate)
            .When(x => x.ContractEndDate.HasValue && x.ContractDate.HasValue)
            .WithMessage(localizer["Val.ExpiryBeforeIssue"].Value);

        RuleForEach(x => x.Dependents)
            .ChildRules(row =>
            {
                row.RuleFor(a => a.ArName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.EnName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.Relation).MaximumLength(50).WithMessage(MaxLength(50));
            });

        RuleForEach(x => x.Allowances)
            .ChildRules(row =>
            {
                row.RuleFor(a => a.ArName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.EnName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.Amount).GreaterThanOrEqualTo(0).When(a => a.Amount.HasValue).WithMessage(required);
            });

        RuleForEach(x => x.FixedDeductions)
            .ChildRules(row =>
            {
                row.RuleFor(a => a.ArName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.EnName).MaximumLength(100).WithMessage(MaxLength(100));
                row.RuleFor(a => a.Amount).GreaterThanOrEqualTo(0).When(a => a.Amount.HasValue).WithMessage(required);
            });

        RuleForEach(x => x.Leaves)
            .ChildRules(row =>
            {
                row.RuleFor(a => a.Days).GreaterThanOrEqualTo(0).When(a => a.Days.HasValue).WithMessage(required);
                row.RuleFor(a => a.Notes).MaximumLength(500).WithMessage(MaxLength(500));
                row.RuleFor(a => a.ToDate)
                    .GreaterThanOrEqualTo(a => a.FromDate)
                    .When(a => a.FromDate.HasValue && a.ToDate.HasValue)
                    .WithMessage(localizer["Val.EndBeforeStart"].Value);
            });

        RuleForEach(x => x.Documents)
            .ChildRules(row =>
            {
                row.RuleFor(a => a.Name)
                    .NotEmpty()
                    .When(a => a.File is not null || !string.IsNullOrWhiteSpace(a.Url))
                    .WithMessage(required);
                row.RuleFor(a => a.Name).MaximumLength(150).WithMessage(MaxLength(150));
                row.RuleFor(a => a.File)
                    .Must(file => file.Length <= 10 * 1024 * 1024)
                    .When(a => a.File is not null)
                    .WithMessage(localizer["Val.FileTooLarge"].Value);
            });

        RuleFor(x => x.BranchId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.JobId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);
    }
}
