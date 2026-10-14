using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Specialization : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public MainOrBranchType Type { get; set; }
    public int? ParentId { get; set; }

    public virtual Specialization Parent { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Technician> Technicians { get; set; } = new List<Technician>();
}
