using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Unit : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Remarks { get; set; }

    public virtual ICollection<ItemUnit> ItemUnits { get; set; } = new HashSet<ItemUnit>();
    public virtual ICollection<StoreItemUnit> StoreItems { get; set; } = new HashSet<StoreItemUnit>();
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new HashSet<BillDetail>();
}
