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

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer["Val.NotNegative"].Value);
    }
}
