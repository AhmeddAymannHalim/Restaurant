using Shared.Enums;

namespace Shared.ViewModels.Commons;

public class TrialBalanceLine
{
    public int AccountId { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public AccountType AccountType { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public decimal Balance => TotalDebit - TotalCredit;
}
