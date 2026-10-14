using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class CustomerTier : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
}
