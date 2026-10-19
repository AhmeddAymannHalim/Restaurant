using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Account : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal CreditBalance { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal? MaxDebitAllowed { get; set; }
    public int? Level { get; set; }
    public string Remarks { get; set; }
    public int? NumberOfSubAccounts { get; set; }
    public bool RequiresTurning { get; set; }
    public bool AccountClass { get; set; }
    public bool RequiresCostCenter { get; set; }
    public DateTime AccDate { get; set; }
    public DateTime AccCheckDate { get; set; }
    public string AccCustomerCode { get; set; }

    [NotMapped]
    public decimal BalanceValue => BalanceType == AccountBalanceType.Debit ? DebitBalance - CreditBalance : CreditBalance - DebitBalance;

    public AccountBalanceType BalanceType { get; set; }

    [NotMapped]
    public AccountKind Kind { get; set; }
    public AccountType AccountType { get; set; }

    public int? EmployeeId { get; set; }
    public int? CurrencyId { get; set; }
    public int? ParentAccountId { get; set; }
    public int? FinalAccountId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual Account ParentAccount { get; set; }
    public virtual Account FinalAccount { get; set; }
    public virtual AccountDetails AccountDetails { get; set; }

    public virtual ICollection<Account> ChildAccounts { get; set; } = new List<Account>();

    public static Expression<Func<Account, decimal>> BalanceExpression =>
        a => a.BalanceType == AccountBalanceType.Debit ? a.DebitBalance - a.CreditBalance : a.CreditBalance - a.DebitBalance;

    public void Adjust(decimal delta)
    {
        var growsDebit = (delta >= 0) == (BalanceType == AccountBalanceType.Debit);
        if (growsDebit)
        {
            DebitBalance += Math.Abs(delta);
        }
        else
        {
            CreditBalance += Math.Abs(delta);
        }
    }
}
