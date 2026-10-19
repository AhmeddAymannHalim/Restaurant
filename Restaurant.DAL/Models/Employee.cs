using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Employee : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string IDNumber { get; set; }
    public string Religion { get; set; }
    public int? NationalityId { get; set; }
    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }
    public string UserId { get; set; }

    public virtual Country Nationality { get; set; }
    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }
    public virtual ApplicationUser User { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    #region HR

    public string Email { get; set; }

    public DateTime? NationalIdIssueDate { get; set; }
    public DateTime? NationalIdExpiryDate { get; set; }
    public DateTime? BorderEntryDate { get; set; }
    public string ProfessionInId { get; set; }
    public string ActualProfession { get; set; }
    public string VisaNumber { get; set; }
    public string Port { get; set; }
    public string IssuingAuthority { get; set; }

    public bool HasBankAccount { get; set; }
    public string AccountNumber { get; set; }
    public string IBAN { get; set; }

    public string SponsorName { get; set; }
    public string SponsorPhone { get; set; }

    public string DrivingLicenseNumber { get; set; }
    public DateTime? DrivingLicenseIssueDate { get; set; }
    public DateTime? DrivingLicenseExpiryDate { get; set; }

    public int AnnualLeaveDays { get; set; }
    public int SickLeaveDays { get; set; }
    public int CasualLeaveDays { get; set; }

    public bool HasTravelTicket { get; set; }
    public TicketType? TicketType { get; set; }
    public string Destination { get; set; }
    public int TicketCount { get; set; }

    public bool HasCar { get; set; }
    public string CarType { get; set; }
    public string CarNumber { get; set; }

    public bool IsSalaryStopped { get; set; }
    public decimal? BasicSalary { get; set; }
    public int? WorkDays { get; set; }
    public decimal? DailyHours { get; set; }
    public decimal? WeeklyHours { get; set; }
    public decimal? MonthlyHours { get; set; }
    public SalaryPaymentMethod PaymentMethod { get; set; }
    public AttendanceMethod AttendanceMethod { get; set; }
    public ShiftType ShiftType { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? DailyRate { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? MinuteRate { get; set; }
    public string SalaryNotes { get; set; }

    public int? ContractTypeId { get; set; }
    public int? AccountId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }

    public virtual ContractType ContractType { get; set; }
    public virtual Account Account { get; set; }
    public virtual Branch Branch { get; set; }
    public virtual Department Department { get; set; }

    public DateTime? HireDate { get; set; }
    public DateTime? ContractDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    public string InsuranceNumber { get; set; }
    public DateTime? InsuranceStartDate { get; set; }
    public DateTime? InsuranceEndDate { get; set; }
    public decimal? InsuranceBasicSalary { get; set; }
    public decimal? InsuranceAllowancesAmount { get; set; }
    public decimal? EmployeeInsurancePercent { get; set; }
    public decimal? EmployeeInsuranceAmount { get; set; }
    public decimal? CompanyInsurancePercent { get; set; }
    public decimal? CompanyInsuranceAmount { get; set; }
    public bool IsActive { get; set; }

    public int? JobId { get; set; }
    public int? SpecializationId { get; set; }
    public int? JobTypeId { get; set; }

    public virtual Job Job { get; set; }
    public virtual Specialization Specialization { get; set; }
    public virtual JobType JobType { get; set; }

    public string InsuranceCardNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Notes { get; set; }

    public bool OverrideDelayRule { get; set; }
    public int? GracePeriodMinutesOverride { get; set; }
    public DelayDeductionMode? DelayDeductionModeOverride { get; set; }
    public int? DelayRuleMinutesOverride { get; set; }
    public decimal? DelayRuleAmountOverride { get; set; }
    public DayFraction? DelayDayFractionOverride { get; set; }
    public decimal? LateDeductionMultiplierOverride { get; set; }
    public bool FullDayEscalationEnabledOverride { get; set; }
    public int? FullDayDeductionThresholdMinutesOverride { get; set; }

    public bool OverrideOvertimeRule { get; set; }
    public int? OvertimeRuleMinutesStartOverride { get; set; }
    public int? OvertimeRuleMinutesOverride { get; set; }
    public decimal? OvertimeRuleAmountOverride { get; set; }
    public OvertimeCalculationMode? OvertimeCalculationModeOverride { get; set; }
    public decimal? OvertimeMultiplierOverride { get; set; }

    public bool OverrideWeeklyOffDays { get; set; }
    public WeeklyOffDays WeeklyOffDaysOverride { get; set; } = WeeklyOffDays.None;
    public bool OverrideOffDayOvertimeRule { get; set; }
    public bool OffDayAsOvertimeOverride { get; set; }

    public bool OverrideEarlyLeaveRule { get; set; }
    public bool EarlyLeaveDeductionEnabledOverride { get; set; }
    public int? EarlyLeaveGraceMinutesOverride { get; set; }

    public int? InsuranceCategoryId { get; set; }
    public int? InsuranceCompanyId { get; set; }

    public virtual InsuranceCategory InsuranceCategory { get; set; }
    public virtual InsuranceCompany InsuranceCompany { get; set; }

    public virtual ICollection<EmployeePhone> Phones { get; set; } = new List<EmployeePhone>();
    public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public virtual ICollection<EmployeeDependent> Dependents { get; set; } = new List<EmployeeDependent>();
    public virtual ICollection<EmployeeLeave> Leaves { get; set; } = new List<EmployeeLeave>();
    public virtual ICollection<EmployeeAllowance> Allowances { get; set; } = new List<EmployeeAllowance>();
    public virtual ICollection<EmployeeFixedDeduction> FixedDeductions { get; set; } = new List<EmployeeFixedDeduction>();
    public virtual ICollection<AttendanceInOut> AttendanceRecords { get; set; } = new List<AttendanceInOut>();

    [NotMapped]
    public List<string> PhoneNumbers => Phones.Select(p => p.PhoneNumber).ToList();
    [NotMapped]
    public List<string> ExistingDocumentUrls => Documents.Select(d => d.Url).ToList();

    #endregion

    #region Restaurant

    [NotMapped]
    public bool HasLoginAccount { get; set; }

    [NotMapped]
    public string Image { get; set; }

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
    public virtual ICollection<Scrounge> Scrounges { get; set; } = new List<Scrounge>();

    #endregion
}
