namespace Shared.ViewModels.Definitions;

public class EmployeeExpenseViewModel
{
    public decimal Incentives { get; set; }
    public decimal Discounts { get; set; }
    public decimal Depts { get; set; }
    public decimal Allowances { get; set; }
    public decimal FixedDeductions { get; set; }
    public bool IsSalaryStopped { get; set; }
    public decimal MainSalary { get; set; }
    public decimal TotalSalary { get; set; }
    public string SalaryType { get; set; }
}
