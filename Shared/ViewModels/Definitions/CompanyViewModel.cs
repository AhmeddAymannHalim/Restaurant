using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class CompanyViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? CurrencyId { get; set; }
    public string CurrencyArName { get; set; }
    public string CurrencyEnName { get; set; }
    public IEnumerable<LookupItem> Currencies { get; set; } = new List<LookupItem>();
}
