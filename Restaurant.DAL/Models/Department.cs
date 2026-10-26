using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Department : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
