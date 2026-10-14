using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class EmployeeDocument : BaseEntity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
