using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeeRequest : BaseEntity
{
    public RequestType RequestType { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.Today;
    public bool IsApproved { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Notes { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual LeaveType LeaveType { get; set; }
    public virtual ICollection<EntryMaster> EntryMasters { get; set; } = new List<EntryMaster>();
}
