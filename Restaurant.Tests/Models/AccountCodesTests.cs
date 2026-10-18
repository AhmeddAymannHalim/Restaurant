using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.Tests.Models;

public class AccountCodesTests
{
    [Fact]
    public void Default_ContainsInventoryAdjustmentAsDebitExpense()
    {
        var def = Assert.Single(AccountCodes.Default, a => a.Code == AccountCodes.InventoryAdjustment);

        Assert.Equal(AccountBalanceType.Debit, def.BalanceType);
        Assert.Equal(AccountCodes.Expenses, def.ParentCode);
    }

    [Fact]
    public void Default_ContainsOpeningBalanceEquityAsCreditEquity()
    {
        var def = Assert.Single(AccountCodes.Default, a => a.Code == AccountCodes.OpeningBalanceEquity);

        Assert.Equal(AccountBalanceType.Credit, def.BalanceType);
        Assert.Equal(AccountCodes.Equity, def.ParentCode);
    }

    [Fact]
    public void Default_HasNoDuplicateCodes()
    {
        var codes = AccountCodes.Default.Select(a => a.Code).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }
}
