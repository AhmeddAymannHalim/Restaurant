using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class EmployeeLeave : BaseEntity
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Days { get; set; }
    public string Notes { get; set; }
    public int EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual LeaveType LeaveType { get; set; }
}
