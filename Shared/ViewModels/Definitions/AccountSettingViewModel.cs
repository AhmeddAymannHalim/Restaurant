using Shared.ViewModels.Commons;

namespace Shared.ViewModels.Definitions;

// the accounts the invoices, orders and vouchers use to create their entries
public class AccountSettingViewModel
{
    public int Id { get; set; }

    // the main accounts that new customers, suppliers, employees... are created under
    public int? ParentCustomerId { get; set; }
    public int? ParentSupplierId { get; set; }
    public int? ParentEmployeeId { get; set; }
    public int? ParentExpensesId { get; set; }
    public int? ParentBankId { get; set; }
    public int? ParentBoxId { get; set; }
    public int? ParentFixedAssetsId { get; set; }

    // invoices: the item account (sales / purchases) and the cash account
    public int? SalesItemAccountId { get; set; }
    public int? SalesPayAccountId { get; set; }
    public int? SalesReturnItemAccountId { get; set; }
    public int? SalesReturnPayAccountId { get; set; }
    public int? PurchasesItemAccountId { get; set; }
    public int? PurchasesPayAccountId { get; set; }
    public int? PurchaseReturnItemAccountId { get; set; }
    public int? PurchaseReturnPayAccountId { get; set; }

    // vouchers: the main account (cash / bank) of each kind
    public int? CustomerPaymentAccountId { get; set; }
    public int? SupplierPaymentAccountId { get; set; }
    public int? CustomerRefundAccountId { get; set; }
    public int? SupplierRefundAccountId { get; set; }
    public int? CustomerOpeningBalanceAccountId { get; set; }
    public int? SupplierOpeningBalanceAccountId { get; set; }

    public IEnumerable<LookupItem> Accounts { get; set; } = new List<LookupItem>();
}
