namespace Shared.ViewModels.Definitions;

public class TableViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public byte NumOfPersons { get; set; }
    public bool IsPending { get; set; }
}
