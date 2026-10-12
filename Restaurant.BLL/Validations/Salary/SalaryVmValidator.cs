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
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.SalaryAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(required);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage(required);
    }
}
