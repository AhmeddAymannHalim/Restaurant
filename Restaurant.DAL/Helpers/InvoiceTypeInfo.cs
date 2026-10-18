namespace Restaurant.DAL.Helpers;

public class InvoiceTypeInfo
{
    public Shared.Enums.InvoiceType Type { get; set; }
    public string Code { get; set; }
    public Shared.Enums.BillType BillType { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public Shared.Enums.EntryType Entry { get; set; }
}
