using FluentValidation;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Shared;
using Shared.ViewModels.Definitions;

namespace Restaurant.BLL.Validations.Product;

public class ProductVmValidator : AbstractValidator<ProductViewModel>
{
    public ProductVmValidator(
        IStringLocalizer<SharedResource> localizer,
        IValidatorService<Restaurant.DAL.Models.Product> validator)
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

        RuleFor(x => x.ProductCategoryId)
            .NotNull()
            .WithMessage(required)
            .GreaterThan(0)
            .WithMessage(required);

        RuleForEach(x => x.Stocks)
            .ChildRules(stock =>
            {
                stock.RuleFor(a => a.StoreId)
                    .GreaterThan(0)
                    .WithMessage(required);
                stock.RuleFor(a => a.Quantity)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(localizer["Val.NotNegative"].Value);
            });

        RuleFor(x => x.Stocks)
            .Must(stocks => stocks.Select(a => a.StoreId).Distinct().Count() == stocks.Count)
            .WithMessage(localizer["Val.StoreRepeated"].Value);

        RuleForEach(x => x.Units)
            .ChildRules(unit =>
            {
                unit.RuleFor(a => a.UnitPriceId)
                    .GreaterThan(0)
                    .WithMessage(required);
                unit.RuleFor(a => a.TransRate)
                    .GreaterThan(0)
                    .WithMessage(localizer["Val.GreaterThanZero"].Value);
                unit.RuleFor(a => a.LastPurchase)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(localizer["Val.NotNegative"].Value);
                unit.RuleForEach(a => a.Prices)
                    .ChildRules(price =>
                    {
                        price.RuleFor(a => a.SellTypeId)
                            .GreaterThan(0)
                            .WithMessage(required);
                        price.RuleFor(a => a.Price)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage(localizer["Val.NotNegative"].Value);
                    });
                unit.RuleFor(a => a.Prices)
                    .Must(prices => prices.Select(a => a.SellTypeId).Distinct().Count() == prices.Count)
                    .WithMessage(localizer["Val.PriceTypeRepeated"].Value);
            });

        RuleFor(x => x.Units)
            .Must(units => units.Select(a => a.UnitPriceId).Distinct().Count() == units.Count)
            .WithMessage(localizer["Val.UnitRepeated"].Value)
            .Must(units => units.Count == 0 || units.Any(a => a.TransRate == 1))
            .WithMessage(localizer["Val.UnitNeedsBase"].Value)
            .Must(units => units.Count == 0 || units.Count(a => a.IsDefault) == 1)
            .WithMessage(localizer["Val.UnitNeedsOneDefault"].Value);

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value);
    }
}
