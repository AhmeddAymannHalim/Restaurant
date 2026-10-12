using Shared.Enums;
using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

public class PaymentViewModel
{
    public int Id { get; set; }
    public string ProcessCode { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int PartnerId { get; set; }
    public decimal Paid { get; set; }
    public string Description { get; set; }

    public string PartnerArName { get; set; }
    public string PartnerEnName { get; set; }

    public IEnumerable<LookupItem> Partners { get; set; } = new List<LookupItem>();
}

public class PaymentRefundViewModel
{
    public int Id { get; set; }
    public int OriginalPaymentId { get; set; }
    public string OriginalPaymentCode { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; }
    public string RequestedByUserName { get; set; }
    public string ApprovedByUserName { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public RefundStatus Status { get; set; }
}

public class PaymentRefundRequestViewModel
{
    public int OriginalPaymentId { get; set; }
    public string OriginalPaymentCode { get; set; }
    public decimal OriginalPaymentAmount { get; set; }
    public decimal RemainingRefundable { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; }

    public List<PaymentRefundViewModel> Refunds { get; set; } = new();
}
