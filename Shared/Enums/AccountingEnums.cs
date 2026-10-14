namespace Shared.Enums;

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}

public enum AccountNature
{
    Debit,
    Credit
}

public enum JournalSourceType
{
    Invoice,
    PaymentVoucher,
    Manual,

    // Customer and supplier vouchers live in separate tables, so their ids need separate source types.
    CustomerPaymentVoucher,
    SupplierPaymentVoucher
}
