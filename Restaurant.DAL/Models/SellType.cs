using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class SellType : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public bool Show { get; set; }
    public bool IsLastPrice { get; set; }
    public decimal? PlusPrice { get; set; }
}
