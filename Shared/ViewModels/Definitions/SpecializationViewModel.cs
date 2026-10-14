using Shared.Enums;
using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class SpecializationViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public MainOrBranchType Type { get; set; } = MainOrBranchType.Main;
    public int? ParentId { get; set; }
    public IEnumerable<LookupItem> Parents { get; set; } = new List<LookupItem>();
}
