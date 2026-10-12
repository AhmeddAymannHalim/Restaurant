using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class ReportRequest
{
    public string Report { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? PartnerId { get; set; }
    public string Type { get; set; }
}

public class ReportViewModel
{
    public string Report { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? PartnerId { get; set; }
    public string Type { get; set; }

    public List<string> Columns { get; set; } = new();

    public List<List<object>> Rows { get; set; } = new();

    public List<ReportTotal> Totals { get; set; } = new();

    public bool UsesPartner { get; set; }
    public string PartnerLabel { get; set; }
    public bool UsesType { get; set; }
    public List<string> Types { get; set; } = new();
    public IEnumerable<LookupItem> Partners { get; set; } = new List<LookupItem>();
}

public class ReportTotal
{
    public string Label { get; set; }
    public decimal Value { get; set; }
}
