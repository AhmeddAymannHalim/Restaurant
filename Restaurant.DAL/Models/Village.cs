namespace Restaurant.DAL.Models;

public class Village : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int TownId { get; set; }

    public virtual Town Town { get; set; }
}
