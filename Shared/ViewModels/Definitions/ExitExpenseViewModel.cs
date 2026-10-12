using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class ExitExpenseViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public int ExpenseId { get; set; }
    public string ExpenseArName { get; set; }
    public string ExpenseEnName { get; set; }
    public string UserName { get; set; }
    public IEnumerable<LookupItem> Expenses { get; set; } = new List<LookupItem>();
}
