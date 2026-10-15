namespace Shared.ViewModels.Definitions;

public class EntryLineViewModel
{
    public int Row { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountArName { get; set; }
    public string AccountEnName { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public string Remarks { get; set; }
}
