using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Payroll : BaseEntity
{
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal? Allowances { get; set; }
    public decimal? FixedDeductions { get; set; }
    public decimal? LateDeductions { get; set; }
    public decimal? EarlyLeaveDeductions { get; set; }
    public decimal? OvertimeBonus { get; set; }
    public decimal? IncentiveBonus { get; set; }
    public decimal? CarryOverAmount { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsApproved { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal? AbsenceDeductions { get; set; }
    public decimal? AdvanceDeductions { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int ProratedDays { get; set; }

    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual ICollection<EntryMaster> EntryMasters { get; set; } = new List<EntryMaster>();
}
