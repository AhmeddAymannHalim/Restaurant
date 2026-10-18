using Shared.Enums;
using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class EmployeeViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public string PhoneNumber { get; set; }

    public List<PhoneViewModel> Phones { get; set; } = new();
    public List<EmployeeDependentViewModel> Dependents { get; set; } = new();
    public List<EmployeeAllowanceViewModel> Allowances { get; set; } = new();
    public List<EmployeeFixedDeductionViewModel> FixedDeductions { get; set; } = new();
    public List<EmployeeLeaveViewModel> Leaves { get; set; } = new();
    public List<EmployeeDocumentViewModel> Documents { get; set; } = new();
    public int TownId { get; set; }
    public string Address { get; set; }

    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }
    public string IDNumber { get; set; }
    public decimal? BasicSalary { get; set; }
    public int BranchId { get; set; }
    public int JobId { get; set; }
    public string Image { get; set; }
    public IFormFile ImageFile { get; set; }

    public string JobArName { get; set; }
    public string JobEnName { get; set; }
    public string BranchArName { get; set; }
    public string BranchEnName { get; set; }
    public string TownArName { get; set; }
    public string TownEnName { get; set; }
    public bool HasLoginAccount { get; set; }

    public string Email { get; set; }
    public string Religion { get; set; }
    public int? NationalityId { get; set; }
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
    public DateTime? HireDate { get; set; }
    public DateTime? ContractDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; }
    public int? AnnualLeaveDays { get; set; }
    public int? SickLeaveDays { get; set; }
    public int? CasualLeaveDays { get; set; }
    public bool HasTravelTicket { get; set; }
    public TicketType? TicketType { get; set; }
    public string Destination { get; set; }
    public int? TicketCount { get; set; }
    public bool HasCar { get; set; }
    public string CarType { get; set; }
    public string CarNumber { get; set; }
    public bool IsSalaryStopped { get; set; }
    public int? WorkDays { get; set; }
    public decimal? DailyHours { get; set; }
    public decimal? WeeklyHours { get; set; }
    public decimal? MonthlyHours { get; set; }
    public SalaryPaymentMethod? PaymentMethod { get; set; }
    public AttendanceMethod? AttendanceMethod { get; set; }
    public ShiftType? ShiftType { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? DailyRate { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? MinuteRate { get; set; }
    public string SalaryNotes { get; set; }
    public string InsuranceNumber { get; set; }
    public DateTime? InsuranceStartDate { get; set; }
    public DateTime? InsuranceEndDate { get; set; }
    public decimal? InsuranceBasicSalary { get; set; }
    public decimal? InsuranceAllowancesAmount { get; set; }
    public decimal? EmployeeInsurancePercent { get; set; }
    public decimal? EmployeeInsuranceAmount { get; set; }
    public decimal? CompanyInsurancePercent { get; set; }
    public decimal? CompanyInsuranceAmount { get; set; }
    public string InsuranceCardNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
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
    public List<WeeklyOffDays> WeeklyOffDaysSelected { get; set; } = new();
    public bool OverrideOffDayOvertimeRule { get; set; }
    public bool OffDayAsOvertimeOverride { get; set; }
    public bool OverrideEarlyLeaveRule { get; set; }
    public bool EarlyLeaveDeductionEnabledOverride { get; set; }
    public int? EarlyLeaveGraceMinutesOverride { get; set; }

    public IEnumerable<LookupItem> Branches { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Jobs { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Nationalities { get; set; } = new List<LookupItem>();
    public IEnumerable<AllowanceTypeViewModel> AllowanceTypes { get; set; } = new List<AllowanceTypeViewModel>();
    public IEnumerable<DeductionTypeViewModel> DeductionTypes { get; set; } = new List<DeductionTypeViewModel>();
    public IEnumerable<LeaveTypeViewModel> LeaveTypes { get; set; } = new List<LeaveTypeViewModel>();
    public int? DepartmentId { get; set; }
    public IEnumerable<LookupItem> Departments { get; set; } = new List<LookupItem>();
    public int? ContractTypeId { get; set; }
    public IEnumerable<LookupItem> ContractTypes { get; set; } = new List<LookupItem>();
    public int? SpecializationId { get; set; }
    public IEnumerable<LookupItem> Specializations { get; set; } = new List<LookupItem>();
    public int? JobTypeId { get; set; }
    public IEnumerable<LookupItem> JobTypes { get; set; } = new List<LookupItem>();
    public int? InsuranceCategoryId { get; set; }
    public IEnumerable<InsuranceCategoryViewModel> InsuranceCategories { get; set; } = new List<InsuranceCategoryViewModel>();
    public int? InsuranceCompanyId { get; set; }
    public IEnumerable<LookupItem> InsuranceCompanies { get; set; } = new List<LookupItem>();
}
