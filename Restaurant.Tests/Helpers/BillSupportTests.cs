using Restaurant.DAL.Helpers;
using Shared.Enums;

namespace Restaurant.Tests.Helpers;

public class BillSupportTests
{
    [Fact]
    public void Types_HasAnEntryForEveryInvoiceTypeValue()
    {
        var allTypes = Enum.GetValues<InvoiceType>();

        foreach (var type in allTypes)
        {
            Assert.True(BillSupport.Types.ContainsKey(type), $"InvoiceType.{type} is missing a BillSupport.Types mapping.");
        }
    }

    [Fact]
    public void Types_HasNoDuplicateCodesOrBillTypes()
    {
        var codes = BillSupport.Types.Values.Select(a => a.Code).ToList();
        var billTypes = BillSupport.Types.Values.Select(a => a.BillType).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
        Assert.Equal(billTypes.Count, billTypes.Distinct().Count());
    }

    [Theory]
    [InlineData(InvoiceType.Shortage, BillType.InventoryDeficit)]
    [InlineData(InvoiceType.Surplus, BillType.InventoryIncrease)]
    [InlineData(InvoiceType.OpeningInventory, BillType.OpeningBalance)]
    public void NewStockOnlyTypes_MapToExpectedBillType(InvoiceType invoiceType, BillType expected)
    {
        Assert.Equal(expected, BillSupport.Types[invoiceType].BillType);
    }
}
