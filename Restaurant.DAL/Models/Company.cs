
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class Company : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public string Email { get; set; }
    public string PostalCode { get; set; }
    public string ImageUrl { get; set; }
    public string ArVillage { get; set; }
    public string EnVillage { get; set; }
    public string ArStreetName { get; set; }
    public string EnStreetName { get; set; }
    public string ArAddress { get; set; }
    public string EnAddress { get; set; }
    public string buildingNo { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string PhoneNumber3 { get; set; }
    public string Note { get; set; }

    #region Zatca
    public string SecondBusinessType { get; set; }
    public string SecondBusinessId { get; set; }
    public SupportedInvoiceTypes ZatcaInvType { get; set; } = 0;
    public bool IsZatcaSupport { get; set; } = false;
    #endregion

    #region ETA
    public bool IsEtaSupport { get; set; } = false;
    public string EtaTaxpayerActivityCode { get; set; }
    #endregion

    #region HR
    public string MapAddress { get; set; }
    public string GoogleLink { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int AllowedRadiusMeters { get; set; } = 50;
    public int DefaultPayrollDay { get; set; } = 1;


    // Delay
    public int? GracePeriodMinutes { get; set; }
    public DelayDeductionMode DelayDeductionMode { get; set; } = DelayDeductionMode.Amount;
    public int? DelayRuleMinutes { get; set; }
    public decimal? DelayRuleAmount { get; set; }
    public DayFraction? DelayDayFraction { get; set; }
    public decimal? LateDeductionMultiplier { get; set; } = 1m;
    public bool FullDayEscalationEnabled { get; set; }
    public int? FullDayDeductionThresholdMinutes { get; set; }

    // Overtime
    public int? OvertimeRuleMinutes { get; set; }
    public int? OvertimeRuleMinutesStart { get; set; }
    public decimal? OvertimeRuleAmount { get; set; }
    public OvertimeCalculationMode OvertimeCalculationMode { get; set; } = OvertimeCalculationMode.Amount;
    public decimal OvertimeMultiplier { get; set; } = 1.0m;
    public bool EarlyLeaveDeductionEnabled { get; set; }
    public int? EarlyLeaveGraceMinutes { get; set; }

    public WeeklyOffDays WeeklyOffDays { get; set; } = WeeklyOffDays.Friday | WeeklyOffDays.Saturday;
    public bool CalculateOffDayAsOvertime { get; set; }
    #endregion

    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }
    public int? TaxId { get; set; }
    public int? CurrencyId { get; set; }

    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }
    public virtual Tax Tax { get; set; }
    public virtual Currency Currency { get; set; }

    public virtual ICollection<CompanyBranch> CompanyBranches { get; set; } = new List<CompanyBranch>();

}
