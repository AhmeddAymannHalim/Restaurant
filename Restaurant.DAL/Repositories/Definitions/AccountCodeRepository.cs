using Restaurant.DAL.IRepositories.Definitions;
using Restaurant.DAL.Models;
using Shared.Enums;

namespace Restaurant.DAL.Repositories.Definitions;

public class AccountCodeRepository : IAccountCodeRepository
{
    public const string Assets = "1000";
    public const string Cash = "1010";
    public const string AccountsReceivable = "1020";
    public const string Inventory = "1030";
    public const string Liabilities = "2000";
    public const string AccountsPayable = "2010";
    public const string EmployeesPayable = "2020";
    public const string Equity = "3000";
    public const string Deposits = "3010";
    public const string Revenue = "4000";
    public const string SalesRevenue = "4010";
    public const string SalesReturns = "4090";
    public const string Expenses = "5000";
    public const string Purchases = "5010";
    public const string PurchaseReturns = "5090";
    public const string OperatingExpenses = "5020";
    public const string Salaries = "5030";
    public const string InventoryAdjustment = "5040";
    public const string OpeningBalanceEquity = "3020";

    public List<DefaultAccount> GetDefaultAccounts()
    {
        var accounts = new List<DefaultAccount>();

        AddAccount(accounts, Assets, "الأصول", "Assets", AccountType.Collective, AccountBalanceType.Debit, null);
        AddAccount(accounts, Liabilities, "الالتزامات", "Liabilities", AccountType.Collective, AccountBalanceType.Credit, null);
        AddAccount(accounts, Equity, "حقوق الملكية", "Equity", AccountType.Collective, AccountBalanceType.Credit, null);
        AddAccount(accounts, Revenue, "الإيرادات", "Revenue", AccountType.Collective, AccountBalanceType.Credit, null);
        AddAccount(accounts, Expenses, "المصروفات", "Expenses", AccountType.Collective, AccountBalanceType.Debit, null);
        AddAccount(accounts, Deposits, "إيداعات", "Deposits", AccountType.Normal, AccountBalanceType.Credit, Equity);
        AddAccount(accounts, OperatingExpenses, "مصروفات التشغيل", "Operating expenses", AccountType.Normal, AccountBalanceType.Debit, Expenses);
        AddAccount(accounts, Salaries, "المرتبات والأجور", "Salaries and wages", AccountType.Normal, AccountBalanceType.Debit, Expenses);
        AddAccount(accounts, Cash, "النقدية والبنك", "Cash and bank", AccountType.Normal, AccountBalanceType.Debit, Assets);
        AddAccount(accounts, AccountsReceivable, "العملاء", "Accounts receivable", AccountType.Normal, AccountBalanceType.Debit, Assets);
        AddAccount(accounts, Inventory, "المخزون", "Inventory", AccountType.Normal, AccountBalanceType.Debit, Assets);
        AddAccount(accounts, AccountsPayable, "الموردون", "Accounts payable", AccountType.Normal, AccountBalanceType.Credit, Liabilities);
        AddAccount(accounts, EmployeesPayable, "مستحقات الموظفين", "Employees payable", AccountType.Normal, AccountBalanceType.Credit, Liabilities);
        AddAccount(accounts, SalesRevenue, "إيرادات المبيعات", "Sales revenue", AccountType.Normal, AccountBalanceType.Credit, Revenue);
        AddAccount(accounts, SalesReturns, "مرتجعات المبيعات", "Sales returns", AccountType.Normal, AccountBalanceType.Debit, Revenue);
        AddAccount(accounts, Purchases, "المشتريات وتكلفة البيع", "Purchases and cost of sales", AccountType.Normal, AccountBalanceType.Debit, Expenses);
        AddAccount(accounts, PurchaseReturns, "مرتجعات المشتريات", "Purchase returns", AccountType.Normal, AccountBalanceType.Credit, Expenses);
        AddAccount(accounts, InventoryAdjustment, "تسويات المخزون", "Inventory adjustments", AccountType.Normal, AccountBalanceType.Debit, Expenses);
        AddAccount(accounts, OpeningBalanceEquity, "الرصيد الافتتاحي", "Opening balance equity", AccountType.Normal, AccountBalanceType.Credit, Equity);

        return accounts;
    }

    public bool IsSystemCode(string code)
    {
        foreach (var account in GetDefaultAccounts())
        {
            if (account.Code == code)
            {
                return true;
            }
        }

        return false;
    }

    private void AddAccount(List<DefaultAccount> accounts, string code, string nameAr, string nameEn, AccountType accountType, AccountBalanceType balanceType, string parentCode)
    {
        var account = new DefaultAccount();
        account.Code = code;
        account.NameAr = nameAr;
        account.NameEn = nameEn;
        account.AccountType = accountType;
        account.BalanceType = balanceType;
        account.ParentCode = parentCode;
        accounts.Add(account);
    }
}
