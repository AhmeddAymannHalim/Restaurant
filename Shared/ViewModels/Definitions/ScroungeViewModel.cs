using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class ScroungeViewModel
{
    public int Id { get; set; }
    public string Info { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public Shared.Enums.MoneyState State { get; set; }
    public string UserName { get; set; }
    public IEnumerable<LookupItem> Employees { get; set; } = new List<LookupItem>();
}
