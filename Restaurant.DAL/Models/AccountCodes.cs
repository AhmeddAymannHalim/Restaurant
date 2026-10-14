using Shared.Enums;

namespace Restaurant.DAL.Models;

/// <summary>Codes of the default chart of accounts the automatic postings rely on.</summary>
public static class AccountCodes
{
    public const string Assets = "1000";
    public const string Cash = "1010";
    public const string AccountsReceivable = "1020";
    public const string Inventory = "1030";
    public const string Liabilities = "2000";
    public const string AccountsPayable = "2010";
    public const string EmployeesPayable = "2020";
    public const string Equity = "3000";
    public const string Revenue = "4000";
    public const string SalesRevenue = "4010";
    public const string SalesReturns = "4090";
    public const string Expenses = "5000";
    public const string Purchases = "5010";
    public const string PurchaseReturns = "5090";

    public record Definition(string Code, string NameAr, string NameEn, AccountType AccountType, AccountBalanceType BalanceType, string ParentCode);

    public static readonly Definition[] Default =
    {
        new(Assets, "الأصول", "Assets", AccountType.Collective, AccountBalanceType.Debit, null),
        new(Liabilities, "الالتزامات", "Liabilities", AccountType.Collective, AccountBalanceType.Credit, null),
        new(Equity, "حقوق الملكية", "Equity", AccountType.Collective, AccountBalanceType.Credit, null),
        new(Revenue, "الإيرادات", "Revenue", AccountType.Collective, AccountBalanceType.Credit, null),
        new(Expenses, "المصروفات", "Expenses", AccountType.Collective, AccountBalanceType.Debit, null),
        new(Cash, "النقدية والبنك", "Cash and bank", AccountType.Normal, AccountBalanceType.Debit, Assets),
        new(AccountsReceivable, "العملاء", "Accounts receivable", AccountType.Normal, AccountBalanceType.Debit, Assets),
        new(Inventory, "المخزون", "Inventory", AccountType.Normal, AccountBalanceType.Debit, Assets),
        new(AccountsPayable, "الموردون", "Accounts payable", AccountType.Normal, AccountBalanceType.Credit, Liabilities),
        new(EmployeesPayable, "مستحقات الموظفين", "Employees payable", AccountType.Normal, AccountBalanceType.Credit, Liabilities),
        new(SalesRevenue, "إيرادات المبيعات", "Sales revenue", AccountType.Normal, AccountBalanceType.Credit, Revenue),
        new(SalesReturns, "مرتجعات المبيعات", "Sales returns", AccountType.Normal, AccountBalanceType.Debit, Revenue),
        new(Purchases, "المشتريات وتكلفة البيع", "Purchases and cost of sales", AccountType.Normal, AccountBalanceType.Debit, Expenses),
        new(PurchaseReturns, "مرتجعات المشتريات", "Purchase returns", AccountType.Normal, AccountBalanceType.Credit, Expenses)
    };
}
