using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class AccountCodeRepositoryTests
{
    private readonly AccountCodeRepository _repository = new AccountCodeRepository();

    [Fact]
    public void GetDefaultAccounts_ContainsInventoryAdjustmentAsDebitExpense()
    {
        var account = Assert.Single(_repository.GetDefaultAccounts(), a => a.Code == AccountCodeRepository.InventoryAdjustment);

        Assert.Equal(AccountBalanceType.Debit, account.BalanceType);
        Assert.Equal(AccountCodeRepository.Expenses, account.ParentCode);
    }

    [Fact]
    public void GetDefaultAccounts_ContainsOpeningBalanceEquityAsCreditEquity()
    {
        var account = Assert.Single(_repository.GetDefaultAccounts(), a => a.Code == AccountCodeRepository.OpeningBalanceEquity);

        Assert.Equal(AccountBalanceType.Credit, account.BalanceType);
        Assert.Equal(AccountCodeRepository.Equity, account.ParentCode);
    }

    [Fact]
    public void GetDefaultAccounts_HasNoDuplicateCodes()
    {
        var codes = _repository.GetDefaultAccounts().Select(a => a.Code).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void GetDefaultAccounts_EveryParentCodeIsAnAccountInTheList()
    {
        var accounts = _repository.GetDefaultAccounts();
        var codes = accounts.Select(a => a.Code).ToList();

        foreach (var account in accounts)
        {
            if (account.ParentCode != null)
            {
                Assert.Contains(account.ParentCode, codes);
            }
        }
    }

    [Fact]
    public void GetDefaultAccounts_MainGroupsHaveNoParent()
    {
        var accounts = _repository.GetDefaultAccounts();

        foreach (var code in new[] { AccountCodeRepository.Assets, AccountCodeRepository.Liabilities, AccountCodeRepository.Equity, AccountCodeRepository.Revenue, AccountCodeRepository.Expenses })
        {
            Assert.Null(accounts.Single(a => a.Code == code).ParentCode);
        }
    }

    [Fact]
    public void GetDefaultAccounts_HasNineteenAccounts()
    {
        Assert.Equal(19, _repository.GetDefaultAccounts().Count);
    }

    [Theory]
    [InlineData("1010", true)]
    [InlineData("5040", true)]
    [InlineData("9999", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSystemCode_OnlyTrueForCodesThatComeWithTheProgram(string code, bool expected)
    {
        Assert.Equal(expected, _repository.IsSystemCode(code));
    }
}
