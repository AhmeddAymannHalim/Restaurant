using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Classification : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Description { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
