using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Group : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public int? ParentId { get; set; }

    public virtual Group Parent { get; set; }
}
