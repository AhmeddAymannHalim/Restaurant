using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class SalaryViewModel
{
    public int Id { get; set; }
    public string SalaryCode { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime SalaryDate { get; set; }
    public decimal SalaryAmount { get; set; }
    public int? EmployeeId { get; set; }
    public string EmployeeArName { get; set; }
    public string EmployeeEnName { get; set; }
    public string UserName { get; set; }
    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
}
