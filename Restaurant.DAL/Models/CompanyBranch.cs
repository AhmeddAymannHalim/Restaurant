using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Replaces the old Branche. Company/Currency links are optional so a single-branch restaurant can ignore them.
public class CompanyBranch : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Address { get; set; }
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }
    public string Mobile1 { get; set; }
    public string Mobile2 { get; set; }
    public int? CompanyId { get; set; }
    public int? CurrencyId { get; set; }

    public virtual Company Company { get; set; }
    public virtual Currency Currency { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
