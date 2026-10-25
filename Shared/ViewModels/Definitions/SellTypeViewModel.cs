namespace Shared.ViewModels.Definitions;

public class SellTypeViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public bool Show { get; set; }
    public bool IsLastPrice { get; set; }
    public decimal? PlusPrice { get; set; }
}
