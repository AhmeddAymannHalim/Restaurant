using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.DAL.Models;

public class Product
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Cost { get; set; }
    public int? ProductCategoryId { get; set; }

    public virtual ProductCategory ProductCategory { get; set; }

    public virtual ICollection<MenuItemComponent> MenuItemComponents { get; set; }

    [NotMapped]
    public List<ProductStock> Stocks { get; set; } = new();
}
