using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Supplier : BaseEntity
{
    public string DetailedAddress { get; set; }
    public string Remark { get; set; }
    public string Email { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string Website { get; set; }
    public string PostalCode { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public string BuildingNo { get; set; }
    public string StreetName { get; set; }
    public decimal? CreditLimit { get; set; }
    public int? CreditDurationInDays { get; set; }
    public PayType BillPayType { get; set; } = PayType.Cash;

    public int? AccountId { get; set; }
    public int? BranchId { get; set; }
    public int? CostCenterId { get; set; }
    public int? EmployeeId { get; set; }
    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }

    public virtual Account Account { get; set; }
    public virtual CompanyBranch Branch { get; set; }
    public virtual CostCenter CostCenter { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }

    #region Restaurant

    public decimal? LastBalance { get; set; }

    public virtual ICollection<PaymentInvoiceSupplier> PaymentInvoices { get; set; } = new List<PaymentInvoiceSupplier>();

    #endregion

    public virtual ICollection<SupplierPhone> Phones { get; set; } = new List<SupplierPhone>();
}
