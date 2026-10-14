using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class AccountDefinitionViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public AccountKind Kind { get; set; } = AccountKind.GeneralLedger;
    public AccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public string ParentArName { get; set; }
    public string ParentEnName { get; set; }
    public int? CurrencyId { get; set; }
    public string Remarks { get; set; }
    public decimal? MaxDebitAllowed { get; set; }
    public bool RequiresCostCenter { get; set; }
    public int? Level { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Balance { get; set; }
    public AccountBalanceType BalanceType { get; set; }
    public bool IsSystem { get; set; }
    public bool IsOwned { get; set; }

    public string Location { get; set; }
    public decimal? BuyValue { get; set; }
    public DateTime? BuyDate { get; set; }
    public DateTime? StartUsingDate { get; set; }
    public int? ProductAge { get; set; }
    public DepreciationType DepreciationType { get; set; } = DepreciationType.None;
    public decimal? ScrapValue { get; set; }
    public int? DepreciationAccountId { get; set; }
    public int? DepreciationExpensesAccountId { get; set; }

    public IEnumerable<LookupItem> Parents { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Currencies { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> PostingAccounts { get; set; } = new List<LookupItem>();
}
