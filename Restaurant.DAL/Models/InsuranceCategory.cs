using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class InsuranceCategory : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal? BasicWage { get; set; }
    public decimal? AllowancesAmount { get; set; }
    public decimal? EmployeePercent { get; set; }
    public decimal? CompanyPercent { get; set; }
}
