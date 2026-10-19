namespace Restaurant.DAL.Models;

public class DeliveryZone : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Fee { get; set; }
}
