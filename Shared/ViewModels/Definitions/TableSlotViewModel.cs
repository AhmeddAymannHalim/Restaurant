namespace Shared.ViewModels.Definitions;

// One table on the booking page: can the guest pick it at the chosen time?
public class TableSlotViewModel
{
    public int Id { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public byte NumOfPersons { get; set; }
    public bool IsReserved { get; set; }
    public bool TooSmall { get; set; }
}
