using Shared.Enums;

namespace Restaurant.DAL.Models;

public class MenuItem : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int MenuSectionId { get; set; }
    public string ArDescription { get; set; }
    public string EnDescription { get; set; }
    public string ImageUrl { get; set; }
    public int StoreId { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Size? Size { get; set; }

    public virtual Store Store { get; set; }
    public virtual MenuSection MenuSection { get; set; }
    public virtual ICollection<MenuItemComponent> Components { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? AvailablePortions { get; set; }

    public decimal TotalCost => Components?.Sum(a => a.Quantity * (a.Product?.Cost ?? 0)) ?? 0;
    public decimal Benefit => Price - TotalCost;
}
