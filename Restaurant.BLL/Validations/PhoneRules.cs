using System.Linq.Expressions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations;

public static class PhoneRules
{
    // fixed ERP phone columns (Customer 1-2, Store 1-3): digits only, 20 at most, none repeated
    public static void AddPhoneNumberRules<T>(
        this AbstractValidator<T> validator,
        IStringLocalizer<SharedResource> localizer,
        params Expression<Func<T, string>>[] numbers)
    {
        foreach (var number in numbers)
        {
            validator.RuleFor(number)
                .MaximumLength(20)
                .WithMessage(string.Format(localizer["Val.MaxLength"].Value, 20))
                .Matches(@"^[0-9+\-\s]*$")
                .WithMessage(localizer["Val.PhoneDigitsOnly"].Value);
        }

        var read = numbers.Select(a => a.Compile()).ToList();
        validator.RuleFor(x => x)
            .Must(model =>
            {
                var filled = read.Select(a => a(model)?.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
                return filled.Distinct().Count() == filled.Count;
            })
            .WithName("Phones")
            .WithMessage(localizer["PhoneAlreadyExists"].Value);
    }

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
