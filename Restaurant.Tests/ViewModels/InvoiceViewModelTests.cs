using Shared.Enums;
using Shared.ViewModels.Definitions;

namespace Restaurant.Tests.ViewModels;

public class InvoiceViewModelTests
{
    [Theory]
    [InlineData(InvoiceType.Purchases, true, true)]
    [InlineData(InvoiceType.Returns, true, true)]
    [InlineData(InvoiceType.Sales, true, false)]
    [InlineData(InvoiceType.SalesReturn, true, false)]
    [InlineData(InvoiceType.Shortage, false, false)]
    [InlineData(InvoiceType.Surplus, false, false)]
    [InlineData(InvoiceType.OpeningInventory, false, false)]
    public void HasPartner_And_IsSupplierInvoice_MatchExpectedShape(InvoiceType type, bool hasPartner, bool isSupplierInvoice)
    {
        var model = new InvoiceViewModel { InvoiceType = type };

        Assert.Equal(hasPartner, model.HasPartner);
        Assert.Equal(isSupplierInvoice, model.IsSupplierInvoice);
    }
}
