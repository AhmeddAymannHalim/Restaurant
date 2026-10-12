using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class PaymentViewModel
{
    public int Id { get; set; }
    public string ProcessCode { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int PartnerId { get; set; }
    public decimal Paid { get; set; }
    public string Description { get; set; }

    public string PartnerArName { get; set; }
    public string PartnerEnName { get; set; }

    public IEnumerable<LookupItem> Partners { get; set; } = new List<LookupItem>();
}
