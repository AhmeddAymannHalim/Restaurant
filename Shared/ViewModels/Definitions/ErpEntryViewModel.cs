using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class ErpEntryViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime EntryDate { get; set; }
    public EntryType? EntryType { get; set; }
    public string SettingArName { get; set; }
    public string SettingEnName { get; set; }
    public decimal Total { get; set; }
    public bool IsPosted { get; set; }
    public string Remarks { get; set; }
    public string CollectEntryCode { get; set; }
    public int? BillId { get; set; }
    public string BillCode { get; set; }

    public bool IsManual { get; set; }

    public List<ErpEntryLineViewModel> Lines { get; set; } = new();
    public IEnumerable<LookupItem> Accounts { get; set; } = new List<LookupItem>();
}
