namespace Restaurant.DAL.Models;

public class Kitchen
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public virtual ICollection<MenuSection> MenuSections { get; set; }
}
