using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Helpers;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Helpers;

public class BillSupportTests
{
    private readonly BillSupportRepository _support = new BillSupportRepository(CreateContext());

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    // ---------- the list of invoice types ----------

    [Fact]
    public void GetInvoiceType_HasAnEntryForEveryInvoiceTypeValue()
    {
        var allTypes = Enum.GetValues<InvoiceType>();

        foreach (var type in allTypes)
        {
            Assert.True(_support.GetInvoiceType(type) != null, $"InvoiceType.{type} is missing from _support.GetAllInvoiceTypes().");
        }
    }

    [Fact]
    public void GetAllInvoiceTypes_HasNoDuplicateCodesOrBillTypes()
    {
        var codes = _support.GetAllInvoiceTypes().Select(a => a.Code).ToList();
        var billTypes = _support.GetAllInvoiceTypes().Select(a => a.BillType).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
        Assert.Equal(billTypes.Count, billTypes.Distinct().Count());
    }

    [Theory]
    [InlineData(InvoiceType.Shortage, BillType.InventoryDeficit)]
    [InlineData(InvoiceType.Surplus, BillType.InventoryIncrease)]
    [InlineData(InvoiceType.OpeningInventory, BillType.OpeningBalance)]
    public void NewStockOnlyTypes_MapToExpectedBillType(InvoiceType invoiceType, BillType expected)
    {
        Assert.Equal(expected, _support.GetInvoiceType(invoiceType).BillType);
    }

    [Fact]
    public void GetInvoiceType_EachInfoKnowsItsOwnType()
    {
        foreach (var type in Enum.GetValues<InvoiceType>())
        {
            Assert.Equal(type, _support.GetInvoiceType(type).Type);
        }
    }

    [Fact]
    public void GetInvoiceBillTypes_HasOneBillTypePerInvoiceType()
    {
        var billTypes = _support.GetInvoiceBillTypes();

        Assert.Equal(Enum.GetValues<InvoiceType>().Length, billTypes.Count);
    }

    // ---------- converting between types ----------

    [Fact]
    public void TypeOf_GivesBackTheInvoiceTypeForItsBillType()
    {
        foreach (var info in _support.GetAllInvoiceTypes())
        {
            Assert.Equal(info.Type, _support.TypeOf(info.BillType));
        }
    }

    [Fact]
    public void TypeOf_BillTypeThatIsNotAnInvoice_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _support.TypeOf(BillType.StockTransfers));
    }

    [Fact]
    public void IsInvoiceBillType_TrueForInvoices_FalseForTransfers()
    {
        Assert.True(_support.IsInvoiceBillType(BillType.Sales));
        Assert.True(_support.IsInvoiceBillType(BillType.Purchases));
        Assert.False(_support.IsInvoiceBillType(BillType.StockTransfers));
    }

    [Theory]
    [InlineData("ORD", BillType.Sales)]
    [InlineData("TRF", BillType.StockTransfers)]
    [InlineData("PUR", BillType.Purchases)]
    [InlineData("SRET", BillType.SalesReturn)]
    public void BillTypeOfKind_KnownCode_GivesTheBillType(string kind, BillType expected)
    {
        Assert.Equal(expected, _support.BillTypeOfKind(kind));
    }

    [Theory]
    [InlineData("")]
    [InlineData("NOPE")]
    public void BillTypeOfKind_UnknownCode_GivesNull(string kind)
    {
        Assert.Null(_support.BillTypeOfKind(kind));
    }

    // ---------- codes stored on items and units ----------

    [Theory]
    [InlineData("RST-P12", 12)]
    [InlineData("RST-P1", 1)]
    [InlineData("RST-P", 0)]
    [InlineData("RST-Pabc", 0)]
    [InlineData("OTHER-12", 0)]
    [InlineData(null, 0)]
    public void ProductIdOf_ReadsTheNumberAfterThePrefix(string code, int expected)
    {
        Assert.Equal(expected, _support.ProductIdOf(code));
    }

    [Theory]
    [InlineData("RST-U7", 7)]
    [InlineData("RST-U", 0)]
    [InlineData("RST-P7", 0)]
    [InlineData(null, 0)]
    public void UnitPriceIdOf_ReadsTheNumberAfterThePrefix(string code, int expected)
    {
        Assert.Equal(expected, _support.UnitPriceIdOf(code));
    }

    // ---------- next bill number ----------

    [Fact]
    public async Task NextBillCodeAsync_NoBillsYet_StartsAtOne()
    {
        using var context = CreateContext();

        var code = await new BillSupportRepository(context).NextBillCodeAsync(billSettingId: 1);

        Assert.Equal("1", code);
    }

    [Fact]
    public async Task NextBillCodeAsync_UsesTheHighestNumberAtTheEndOfTheCodes()
    {
        // Arrange: codes like "INV-0042" end with their number. A code with no number is ignored.
        using var context = CreateContext();
        context.BillMasters.Add(new BillMaster { BillSettingId = 1, Code = "INV-0007" });
        context.BillMasters.Add(new BillMaster { BillSettingId = 1, Code = "INV-0042" });
        context.BillMasters.Add(new BillMaster { BillSettingId = 1, Code = "no number" });
        await context.SaveChangesAsync();

        // Act
        var code = await new BillSupportRepository(context).NextBillCodeAsync(1);

        // Assert
        Assert.Equal("43", code);
    }

    [Fact]
    public async Task NextBillCodeAsync_OtherBillSettingsAreNotCounted()
    {
        using var context = CreateContext();
        context.BillMasters.Add(new BillMaster { BillSettingId = 1, Code = "5" });
        context.BillMasters.Add(new BillMaster { BillSettingId = 2, Code = "900" });
        await context.SaveChangesAsync();

        var code = await new BillSupportRepository(context).NextBillCodeAsync(1);

        Assert.Equal("6", code);
    }
}
