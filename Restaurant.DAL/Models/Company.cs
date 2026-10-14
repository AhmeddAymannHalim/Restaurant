namespace Restaurant.DAL.Models;

public class Company : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? CurrencyId { get; set; }

    public virtual Currency Currency { get; set; }
}
