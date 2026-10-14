using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Job : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }

    public int? JobTypeId { get; set; }
    public virtual JobType JobType { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
