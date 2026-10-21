using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.ExitExpense;

public class ExitExpenseVmValidator : AbstractValidator<ExitExpenseViewModel>
{
    public ExitExpenseVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.ExitExpense> validator)
    {
        var required = localizer["Val.RequiredField"].Value;

        RuleFor(x => x.ExpenseId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage(required);
    }
}
