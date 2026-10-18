using Restaurant.BLL.Validations.Invoice;
using Restaurant.Tests.Fakes;
using Shared;
using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests.Validations;

public class InvoiceVmValidatorTests
{
    private static InvoiceVmValidator CreateValidator() => new(new FakeStringLocalizer<SharedResource>());

    private static InvoiceViewModel ValidBaseModel(InvoiceType type) => new()
    {
        InvoiceType = type,
        StoreId = 1,
        Paid = 0,
        Discount = 0,
        Items =
        {
            new InvoiceItemViewModel { ProductId = 1, UnitPriceId = 1, Quantity = 1, Price = 10 }
        }
    };

    [Theory]
    [InlineData(InvoiceType.Shortage)]
    [InlineData(InvoiceType.Surplus)]
    [InlineData(InvoiceType.OpeningInventory)]
    public void PartnerLessTypes_DoNotRequireSupplierOrCustomer(InvoiceType type)
    {
        var model = ValidBaseModel(type);

        var result = CreateValidator().Validate(model);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.SupplierId));
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.CustomerId));
    }

    [Fact]
    public void Purchases_RequiresSupplierId()
    {
        var model = ValidBaseModel(InvoiceType.Purchases);

        var result = CreateValidator().Validate(model);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.SupplierId));
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.CustomerId));
    }

    [Fact]
    public void Sales_RequiresCustomerId()
    {
        var model = ValidBaseModel(InvoiceType.Sales);

        var result = CreateValidator().Validate(model);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.CustomerId));
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.SupplierId));
    }

    [Fact]
    public void Purchases_WithSupplierId_PassesPartnerRule()
    {
        var model = ValidBaseModel(InvoiceType.Purchases);
        model.SupplierId = 1;

        var result = CreateValidator().Validate(model);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(InvoiceViewModel.SupplierId));
    }
}
