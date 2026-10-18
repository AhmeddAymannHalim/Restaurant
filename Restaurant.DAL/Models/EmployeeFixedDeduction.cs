using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeeFixedDeduction : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal Amount { get; set; }
    public int EmployeeId { get; set; }
    public int? DeductionTypeId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual DeductionType DeductionType { get; set; }
}
