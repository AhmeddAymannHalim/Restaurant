using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

// Standard, Gold, Platinum... Minimal shape; replaced by the full ERP class when merged.
public class CustomerTier : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
}
