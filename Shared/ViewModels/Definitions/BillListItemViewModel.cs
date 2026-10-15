namespace Shared.ViewModels.Definitions;

public class BillListItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public string SettingCode { get; set; }
    public string SettingArName { get; set; }
    public string SettingEnName { get; set; }
    public string PartnerArName { get; set; }
    public string PartnerEnName { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
    public bool IsEntryPosted { get; set; }
    public bool IsCashier { get; set; }
    public bool CanPost { get; set; }
    public bool IsEditable { get; set; }
}
