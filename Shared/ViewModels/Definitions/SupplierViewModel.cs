using Shared.ViewModels.Commons;
using Shared.Enums;
namespace Shared.ViewModels.Definitions;

public class SupplierViewModel
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public decimal Balance { get; set; }

    // ---- the rest of the table columns
    public string Email { get; set; }
    public string PhoneNumber2 { get; set; }
    public string Website { get; set; }
    public string StreetName { get; set; }
    public string BuildingNo { get; set; }
    public string PostalCode { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public PayType BillPayType { get; set; } = PayType.Cash;
    public decimal? CreditLimit { get; set; }
    public int? CreditDurationInDays { get; set; }
    public int? CompanyBranchId { get; set; }
    public int? EmployeeId { get; set; }
    public string Remark { get; set; }
    public int? TownId { get; set; }
    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }

    public IEnumerable<LookupItem> Branches { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
    public int? CostCenterId { get; set; }
    public IEnumerable<LookupItem> CostCenters { get; set; } = new List<LookupItem>();
}
