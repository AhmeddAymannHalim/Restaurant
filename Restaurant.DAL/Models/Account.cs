using Restaurant.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Data.Models;

public class Account
{
    [Key]
    public int AccId { get; set; }
    public string AccCode { get; set; } = null!;
    public string AccArName { get; set; } = null!;
    public string AccEnName { get; set; } = null!;
    public byte AccTypeId { get; set; }
    public DateTime AccDate { get; set; }
    public DateTime AccCheckDate { get; set; }
    public byte AccStateId { get; set; }
    public double AccCredit { get; set; }
    public double AccDebit { get; set; }
    public double? AccMaxDebit { get; set; }
    public short CurrencyId { get; set; }
    public int? ParentAccId { get; set; }
    public int? FinalAccId { get; set; }
    public int? AccLevel { get; set; }
    public string AccRemarks { get; set; }
    public short AccNsons { get; set; }
    public string AccTurnningYesOrNo { get; set; } = null!;
    public byte AccDebitOrCreditOrWithout { get; set; }
    public string AccCustomerCode { get; set; }
    public bool? Class { get; set; }
    public bool? AccMustCostCenter { get; set; }


    public virtual AccountsType AccType { get; set; }

    public virtual AccountState AccState { get; set; }

    public virtual Currency Currency { get; set; }
}   


