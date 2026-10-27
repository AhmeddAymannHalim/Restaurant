using Shared.ViewModels.Commons;
using Shared.Enums;
namespace Shared.ViewModels.Definitions;

public class CustomerViewModel
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneNumber2 { get; set; }
    public decimal Balance { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int InvoiceCount { get; set; }
    public bool HasBalance => Debit != 0 || Credit != 0;
    public int? TownId { get; set; }

    public string TownArName { get; set; }
    public string TownEnName { get; set; }

    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }

    public string Email { get; set; }
    public string Website { get; set; }
    public string Village { get; set; }
    public string StreetName { get; set; }
    public string BuildingNo { get; set; }
    public string PostalCode { get; set; }
    public string MapAddress { get; set; }
    public string GoogleLink { get; set; }
    public string IDNumber { get; set; }
    public string Religion { get; set; }
    public int? NationalityId { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public string SecondBusinessType { get; set; }
    public string SecondBusinessId { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public PayType BillPayType { get; set; } = PayType.Cash;
    public decimal? CreditLimit { get; set; }
    public int? CreditDurationInDays { get; set; }
    public int? CreditDurationInMonth { get; set; }
    public int? CreditDurationInYears { get; set; }
    public int? CompanyBranchId { get; set; }
    public int? EmployeeId { get; set; }
    public string BarCodeNumeric { get; set; }
    public decimal LoyaltyPointsNumber { get; set; }
    public decimal LoyaltyPointsExpiryDays { get; set; }
    public string ImageUrl { get; set; }
    public IFormFile ImageFile { get; set; }
    public string Remark { get; set; }

    public IEnumerable<LookupItem> Nationalities { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Branches { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
    public int? CostCenterId { get; set; }
    public IEnumerable<LookupItem> CostCenters { get; set; } = new List<LookupItem>();
    public int? ClassificationId { get; set; }
    public IEnumerable<LookupItem> Classifications { get; set; } = new List<LookupItem>();
    public int? CustomerTierId { get; set; }
    public IEnumerable<LookupItem> CustomerTiers { get; set; } = new List<LookupItem>();
    public int? SpecializationId { get; set; }
    public IEnumerable<LookupItem> Specializations { get; set; } = new List<LookupItem>();
}
