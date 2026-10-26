using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeeAllowance : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Amount { get; set; }
    public int EmployeeId { get; set; }
    public int? AllowanceTypeId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual AllowanceType AllowanceType { get; set; }
}
