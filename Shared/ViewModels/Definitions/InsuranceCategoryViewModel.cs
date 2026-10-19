namespace Shared.ViewModels.Definitions;

public class InsuranceCategoryViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal? BasicWage { get; set; }
    public decimal? AllowancesAmount { get; set; }
    public decimal? EmployeePercent { get; set; }
    public decimal? CompanyPercent { get; set; }
}
