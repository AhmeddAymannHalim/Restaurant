using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class AllowanceType : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public decimal? Amount { get; set; }
}
