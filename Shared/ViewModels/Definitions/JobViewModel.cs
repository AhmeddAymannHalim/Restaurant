using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class JobViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? JobTypeId { get; set; }
    public IEnumerable<LookupItem> JobTypes { get; set; } = new List<LookupItem>();
}
