using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Minimal shape; replaced by the full ERP class when merged.
public class EmployeeAllowance : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Amount { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
