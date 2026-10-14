using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class CostCenter : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string Remarks { get; set; }
    public int? ParentId { get; set; }

    public virtual CostCenter Parent { get; set; }
}
