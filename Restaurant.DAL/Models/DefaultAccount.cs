using Shared.Enums;

namespace Restaurant.DAL.Models;

// One account of the chart of accounts that comes with the program.
public class DefaultAccount
{
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public AccountType AccountType { get; set; }
    public AccountBalanceType BalanceType { get; set; }
    public string ParentCode { get; set; }
}
