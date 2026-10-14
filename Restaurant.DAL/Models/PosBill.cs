using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class PosBill
{
    public int Id { get; set; }
    public string OrderCode { get; set; }
    public decimal Service { get; set; }
    public decimal Tax { get; set; }
    public OrderType Type { get; set; }
    public string SellerId { get; set; }
    public string ReceiverId { get; set; }
    public bool Cash { get; set; }
    public Shared.Enums.PaymentMethod? PaymentMethod { get; set; }
    public bool OnAccount { get; set; }
    public bool Complete { get; set; }
    public bool FinishShift { get; set; }
    public string CustomerName { get; set; } = "عميل نقدي";

    public int? TableId { get; set; }
    public int? CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public int? DeliveryZoneId { get; set; }
    public decimal DeliveryFee { get; set; }

    public virtual BillMaster Bill { get; set; }
    public virtual ApplicationUser Seller { get; set; }
    public virtual ApplicationUser Receiver { get; set; }
    public virtual Table Table { get; set; }
    public virtual Customer Customer { get; set; }
    public virtual Employee Employee { get; set; }
    public virtual DeliveryZone DeliveryZone { get; set; }
    public virtual ICollection<PosBillLine> Lines { get; set; } = new List<PosBillLine>();

    [NotMapped]
    public DateTime OrderDate => Bill?.Date ?? default;

    [NotMapped]
    public decimal Discount => Bill?.BillDiscountValue ?? 0;

    [NotMapped]
    public decimal TotalPrice => Bill?.TotalBillAmountAfterDiscountAndExtra ?? 0;
}
