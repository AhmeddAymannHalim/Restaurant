using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeeDependent : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Relation { get; set; }
    public DateTime? BirthDate { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
