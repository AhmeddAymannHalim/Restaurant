using System.Linq.Expressions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations;

public static class PhoneRules
{
    public static void AddPhoneRules<T>(
        this AbstractValidator<T> validator,
        Func<T, string> primary,
        Expression<Func<T, IEnumerable<PhoneViewModel>>> extras,
        IStringLocalizer<SharedResource> localizer)
    {
        var read = extras.Compile();
        validator.RuleForEach<PhoneViewModel>(extras)
            .ChildRules(phone =>
            {
                phone.RuleFor(a => a.PhoneNumber)
                    .MaximumLength(20)
                    .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 20))
                    .Matches(@"^[0-9+\-\s]*$")
                    .WithMessage(localizer["Val.PhoneDigitsOnly"].Value);
            });

        validator.RuleFor(x => x)
            .Must(model =>
            {
                var numbers = read(model)
                    .Select(a => a.PhoneNumber?.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
                var main = primary(model)?.Trim();
                return numbers.Distinct().Count() == numbers.Count && !numbers.Contains(main);
            })
            .WithName("Phones")
            .WithMessage(localizer["PhoneAlreadyExists"].Value);
    }
}
