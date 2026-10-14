using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Department : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
