using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class Tax : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public double? Value { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
