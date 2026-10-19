
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class CompanyBranch : BaseEntity
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


    public int? CompanyId { get; set; }
    public int? ParentId { get; set; }
    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }
    public int? TaxId { get; set; }
    public int? CurrencyId { get; set; }
    public int? CostCenterId { get; set; }


    public virtual Company Company { get; set; }
    public virtual CompanyBranch Parent { get; set; }
    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }
    public virtual Tax Tax { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual CostCenter CostCenter { get; set; }

}
