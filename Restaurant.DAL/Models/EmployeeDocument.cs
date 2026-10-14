using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeeDocument : BaseEntity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
