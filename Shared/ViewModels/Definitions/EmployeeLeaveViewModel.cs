namespace Shared.ViewModels.Definitions;

public class EmployeeLeaveViewModel
{
    public int Id { get; set; }
    public int? LeaveTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Days { get; set; }
    public string Notes { get; set; }
}
