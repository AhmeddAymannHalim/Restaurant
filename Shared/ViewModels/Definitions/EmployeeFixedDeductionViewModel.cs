namespace Shared.ViewModels.Definitions;

public class EmployeeFixedDeductionViewModel
{
    public int Id { get; set; }
    public int? DeductionTypeId { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal? Amount { get; set; }
}
