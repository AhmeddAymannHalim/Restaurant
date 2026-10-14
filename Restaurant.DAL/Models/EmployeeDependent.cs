using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class EmployeeDependent : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Relation { get; set; }
    public DateTime? BirthDate { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
