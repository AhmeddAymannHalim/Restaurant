using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EntrySetting : BaseEntity
{
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string ArAbbreviation { get; set; }
    public string EnAbbreviation { get; set; }
    public bool AutoPosting { get; set; }
    public EntryType EntryType { get; set; }
    public bool CostCenterBelong { get; set; }
    public bool ShowTheLastBalance { get; set; }

    public int? AccountId { get; set; }
    public int? CurrencyId { get; set; }

    public virtual Account Account { get; set; }
    public virtual Currency Currency { get; set; }
}
