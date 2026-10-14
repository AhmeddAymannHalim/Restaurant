using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Store : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string ArName_Abbr { get; set; }
    public string EnName { get; set; }
    public string EnName_Abbr { get; set; }
    public string Address { get; set; }
    public string Note { get; set; }
    public string PhoneNumber1 { get; set; }
    public string PhoneNumber2 { get; set; }
    public string PhoneNumber3 { get; set; }
    public string Printer { get; set; }
    public string Village { get; set; }

    public int? ParentId { get; set; }
    public int? BranchId { get; set; }
    public int? EmployeeId { get; set; }
    public int? CountryId { get; set; }
    public int? GovId { get; set; }
    public int? TownId { get; set; }

    public virtual Store Parent { get; set; }
    public virtual CompanyBranch Branch { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual Country Country { get; set; }
    public virtual Governorate Gov { get; set; }
    public virtual Town Town { get; set; }

    public virtual ICollection<StoreItemUnit> StoreItems { get; set; } = new HashSet<StoreItemUnit>();
    public virtual ICollection<ItemLocation> ItemLocations { get; set; } = new HashSet<ItemLocation>();
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new HashSet<BillDetail>();

    #region Restaurant

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public virtual ICollection<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();

    #endregion
}
