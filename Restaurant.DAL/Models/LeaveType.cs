using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class LeaveType : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? DaysPerYear { get; set; }
}
