using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

/// <summary>Customer details. Code, names and balance live on the linked Account (created together with the customer).</summary>
public class Customer : BaseEntity
{
    public string Village { get; set; }
    public string DetailedAddress { get; set; }
    public string MapAddress { get; set; }
    public string GoogleLink { get; set; }
    public string Remark { get; set; }
    public string Email { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string Website { get; set; }
    public string ImageUrl { get; set; }
    public string PostalCode { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public string IDNumber { get; set; }
    public string Religion { get; set; }
    public string BuildingNo { get; set; }
    public string StreetName { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? CreditDurationInDays { get; set; }
    public int? CreditDurationInMonth { get; set; }
    public int? CreditDurationInYears { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public PayType BillPayType { get; set; } = PayType.Cash;
    public string BarCodeNumeric { get; set; }
    public decimal LoyaltyPointsNumber { get; set; } = 0;
    public decimal LoyaltyPointsExpiryDays { get; set; } = 0;

    #region Zatca
    public string SecondBusinessType { get; set; }
    public string SecondBusinessId { get; set; }
    #endregion

    public int? CenterId { get; set; }
    public int? AccountId { get; set; }
    public int? CompanyBranchId { get; set; }
    public int? SellTypeId { get; set; }
    public int? CostCenterId { get; set; }
    public int? EmployeeId { get; set; }
    public int? NationalityId { get; set; }
    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }
    public int? ClassificationId { get; set; }
    public int? CustomerTierId { get; set; }
    public int? SpecializationId { get; set; }

    public virtual Account Account { get; set; }
    public virtual CompanyBranch CompanyBranch { get; set; }
    public virtual SellType SellType { get; set; }
    public virtual CostCenter CostCenter { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Country Nationality { get; set; }
    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }
    public virtual Classification Classification { get; set; }
    public virtual Specialization Specialization { get; set; }
    public virtual CustomerTier CustomerTier { get; set; }

    #region Restaurant

    public virtual ICollection<PaymentInvoiceCustomer> PaymentInvoices { get; set; } = new List<PaymentInvoiceCustomer>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    #endregion
}
