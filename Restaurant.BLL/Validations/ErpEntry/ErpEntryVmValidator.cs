using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.ErpEntry;

public class ErpEntryVmValidator : AbstractValidator<ErpEntryViewModel>
{
    public ErpEntryVmValidator(IStringLocalizer<SharedResource> localizer)
    {
        var required = localizer["Val.RequiredField"].Value;
        string MaxLength(int length) => string.Format(localizer["Val.MaxLength"].Value, length);

        RuleFor(x => x.EntryDate)
            .NotEmpty()
            .WithMessage(required);

        RuleFor(x => x.Remarks)
            .MaximumLength(250)
            .WithMessage(MaxLength(250));

        RuleFor(x => x.Lines)
            .Must(lines => lines != null && lines.Count(a => a.AccountId > 0 && (a.Debit.GetValueOrDefault() > 0 || a.Credit.GetValueOrDefault() > 0)) >= 2)
            .WithMessage(localizer["JournalNeedsTwoLines"].Value);

        RuleFor(x => x.Lines)
            .Must(lines => lines == null || lines
                .Where(a => a.AccountId > 0 && (a.Debit.GetValueOrDefault() > 0 || a.Credit.GetValueOrDefault() > 0))
                .All(a => (a.Debit.GetValueOrDefault() > 0) != (a.Credit.GetValueOrDefault() > 0) && a.Debit.GetValueOrDefault() >= 0 && a.Credit.GetValueOrDefault() >= 0))
            .WithMessage(localizer["JournalLineInvalid"].Value);

        RuleFor(x => x.Lines)
            .Must(lines => lines == null || Math.Round(lines.Where(a => a.AccountId > 0).Sum(a => a.Debit.GetValueOrDefault()), 2)
                == Math.Round(lines.Where(a => a.AccountId > 0).Sum(a => a.Credit.GetValueOrDefault()), 2))
            .WithMessage(localizer["JournalNotBalanced"].Value);

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(a => a.Remarks)
                .MaximumLength(250)
                .WithMessage(MaxLength(250));
        });
    }
}
