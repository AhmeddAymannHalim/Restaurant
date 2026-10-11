namespace Shared.ViewModels.Definitions;

public class DepositViewModel
{
    public int Id { get; set; }
    public string ProcessCode { get; set; }
    public string DeposerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedTime { get; set; }
    public string Description { get; set; }
    public string UserName { get; set; }
}
