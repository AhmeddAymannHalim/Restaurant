using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class CompanyViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }
    public string Email { get; set; }
    public string VatNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public int? TaxId { get; set; }
    public IEnumerable<LookupItem> Taxes { get; set; } = new List<LookupItem>();
    public int? CurrencyId { get; set; }
    public string CurrencyArName { get; set; }
    public string CurrencyEnName { get; set; }
    public IEnumerable<LookupItem> Currencies { get; set; } = new List<LookupItem>();
}
