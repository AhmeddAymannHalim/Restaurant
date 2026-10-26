using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Salary;

public class SalaryVmValidator : AbstractValidator<SalaryViewModel>
{
    public SalaryVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Salary> validator)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.EmployeeId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.SalaryAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value);

        // A blank date posts as 0001-01-01, which NotEmpty treats as missing
        RuleFor(x => x.FromDate)
            .NotEmpty()
            .WithMessage(required);

        RuleFor(x => x.ToDate)
            .NotEmpty()
            .WithMessage(required)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage(localizer["Val.ToDateBeforeFromDate"].Value);
    }
}
