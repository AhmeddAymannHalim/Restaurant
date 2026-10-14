using Shared.Enums;

namespace Restaurant.DAL.Models;

public class AccountDetails : BaseEntity
{
    public string Location { get; set; }
    public string FileUrl { get; set; }
    public decimal? BuyValue { get; set; }
    public DateTime? BuyDate { get; set; }
    public DateTime? StartUsingDate { get; set; }
    public int? ParentAccountId { get; set; }
    public int? MainParentAccountId { get; set; }
    public int? ProductAge { get; set; }
    public DepreciationType DepreciationType { get; set; }
    public int? DepreciationAccountId { get; set; }
    public int? DepreciationExpensesAccountId { get; set; }
    public decimal? ScrapValue { get; set; }

    public int? AccountId { get; set; }
    public virtual Account Account { get; set; }
}
