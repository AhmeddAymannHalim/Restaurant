using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Governorate : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int CountryId { get; set; }

    public virtual Country Country { get; set; }
    public virtual ICollection<Town> Towns { get; set; } = new List<Town>();
}
