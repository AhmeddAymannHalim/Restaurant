using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class ContractType : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
}
