using Shared.Enums;

namespace Restaurant.DAL.Models;

public class PaymentMethod : BaseEntity
{
    public byte RowNumber { get; set; }
    public int? AccountPayId { get; set; }
    public int? BillMasterId { get; set; }
    public decimal AmountPaided { get; set; }

    public virtual Account Account { get; set; }
    public virtual BillMaster BillMaster { get; set; }
}
