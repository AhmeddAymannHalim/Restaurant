using Shared.Enums;

namespace Restaurant.DAL.Models;

public class LeaveType : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? DaysPerYear { get; set; }
}
