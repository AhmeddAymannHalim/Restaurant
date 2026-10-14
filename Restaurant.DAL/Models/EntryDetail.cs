using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class EntryDetail : BaseEntity
{
    public short EntryRowCode { get; set; }
    public decimal Credit { get; set; }
    public decimal Debit { get; set; }
    public string EntryDetailsRemarks { get; set; }

    public int? AccountId { get; set; }
    public int? EntryMasterId { get; set; }
    public int? CostCenterId { get; set; }

    public virtual Account Account { get; set; }
    public virtual EntryMaster EntryMaster { get; set; }
    public virtual CostCenter CostCenter { get; set; }
}
