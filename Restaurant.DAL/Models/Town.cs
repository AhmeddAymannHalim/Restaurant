using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Town : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int GovernorateId { get; set; }

    public virtual Governorate Governorate { get; set; }
    public virtual ICollection<Village> Villages { get; set; } = new List<Village>();
}
