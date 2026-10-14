using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EmployeePhone : BaseEntity
{
    public string PhoneNumber { get; set; }
    public int EmployeeId { get; set; }

    public virtual Employee Employee { get; set; }
}
