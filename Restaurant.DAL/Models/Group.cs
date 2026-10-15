
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class Group : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public MainOrBranchType GroupType { get; set; }
    public string Note { get; set; }
    public bool StopGroupTransactions { get; set; }
    public bool SalesPointProgress { get; set; }
    public int? ParentId { get; set; }
    public virtual Group Parent { get; set; }
    public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
}
