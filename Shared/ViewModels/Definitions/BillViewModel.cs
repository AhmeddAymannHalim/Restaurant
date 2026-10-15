namespace Shared.ViewModels.Definitions;

public class BillViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public string SettingCode { get; set; }
    public string SettingArName { get; set; }
    public string SettingEnName { get; set; }
    public string PartnerArName { get; set; }
    public string PartnerEnName { get; set; }

    public decimal ItemsTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Extras { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Profit { get; set; }
    public bool IsEntryPosted { get; set; }
    public bool IsStorageEffected { get; set; }
    public bool IsCashier { get; set; }
    public bool CanPost { get; set; }
    public bool IsEditable { get; set; }

    public List<BillLineViewModel> Lines { get; set; } = new();
    public List<EntryViewModel> Entries { get; set; } = new();
}
