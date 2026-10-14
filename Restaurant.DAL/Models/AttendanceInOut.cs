using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class AttendanceInOut : BaseEntity
{
    public DateTime Date { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
