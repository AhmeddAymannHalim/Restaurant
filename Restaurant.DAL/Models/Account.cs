using Shared.Enums;

namespace Restaurant.DAL.Models;

public class Account
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string NameAr { get; set; }
    public string NameEn { get; set; }
    public AccountType AccountType { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Account ParentAccount { get; set; }
    public virtual ICollection<Account> ChildAccounts { get; set; }
    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; }
}
