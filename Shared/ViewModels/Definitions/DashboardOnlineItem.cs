namespace Shared.ViewModels.Definitions;

public class DashboardOnlineItem
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public int? TableId { get; set; }
    public string TableArName { get; set; }
    public string TableEnName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public List<DashboardLine> Items { get; set; } = new();
}
