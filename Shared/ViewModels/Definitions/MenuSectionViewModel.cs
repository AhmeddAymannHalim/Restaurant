using Shared.ViewModels.Commons;
namespace Shared.ViewModels.Definitions;

public class MenuSectionViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? KitchenId { get; set; }
    public string KitchenArName { get; set; }
    public string KitchenEnName { get; set; }
    public IEnumerable<LookupItem> Kitchens { get; set; } = new List<LookupItem>();
}
