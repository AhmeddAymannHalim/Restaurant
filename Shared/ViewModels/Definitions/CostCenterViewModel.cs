using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class CostCenterViewModel
{
    public string ParentArName { get; set; }
    public string ParentEnName { get; set; }
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Remarks { get; set; }
    public int? ParentId { get; set; }
    public IEnumerable<LookupItem> Parents { get; set; } = new List<LookupItem>();
}
