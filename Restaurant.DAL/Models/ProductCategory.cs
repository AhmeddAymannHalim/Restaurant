namespace Restaurant.DAL.Models;

public class ProductCategory
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}
