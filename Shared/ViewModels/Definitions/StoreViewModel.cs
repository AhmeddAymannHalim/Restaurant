using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class StoreViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public int? StorekeeperId { get; set; }
    public string StorekeeperName { get; set; }
    public IEnumerable<LookupItem> Storekeepers { get; set; } = new List<LookupItem>();
}
