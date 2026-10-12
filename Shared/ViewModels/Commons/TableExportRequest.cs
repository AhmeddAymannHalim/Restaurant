namespace Shared.ViewModels.Commons;

public class TableExportRequest
{
    public string Title { get; set; }
    public string FileName { get; set; }
    public bool IsRightToLeft { get; set; }
    public List<string> Headings { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}
