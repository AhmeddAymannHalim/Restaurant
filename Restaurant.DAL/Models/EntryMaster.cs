using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EntryMaster : BaseEntity
{
    public string Code { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal TotalBalance { get; set; }
    public double CurrencyRate { get; set; }
    public string CollectEntryCode { get; set; }
    public bool IsPosted { get; set; }
    public string ChequeNumber { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string EntryRemarks { get; set; }
    public int ForKest { get; set; }
    public bool Paid { get; set; }
    public int EntryCurrencyDifference { get; set; }
    public bool Disable { get; set; }
    public ChequeType ChequeType { get; set; }
    public ChequePaperType ChequePaperType { get; set; }
    public string BankName { get; set; }
    public MoneyType MoneyType { get; set; }

    public int? AccountBoxOrBankId { get; set; }
    public int? BillId { get; set; }
    public int? CurrencyId { get; set; }
    public int? EmployeeId { get; set; }
    public int? GlobalCostCenterId { get; set; }
    public string UserId { get; set; }
    public int? EntrySettingId { get; set; }
    public int? PayrollId { get; set; }
    public int? EmployeeRequestId { get; set; }

    public virtual Account AccountBoxOrBank { get; set; }
    public virtual BillMaster Bill { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual CostCenter GlobalCostCenter { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual EntrySetting EntrySetting { get; set; }
    public virtual Payroll Payroll { get; set; }
    public virtual EmployeeRequest EmployeeRequest { get; set; }

    public virtual ICollection<EntryDetail> EntryDetails { get; set; } = new HashSet<EntryDetail>();
}
