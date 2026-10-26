using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Country : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArNationality { get; set; }
    public string EnNationality { get; set; }

    public virtual ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
}
