namespace Restaurant.DAL.Models;

public class Currency : BaseEntity
{
    public string Code { get; set; }
    public string ArName { get; set; }
    public string EnName { get; set; }
    public string SubArName { get; set; }
    public string SubEnName { get; set; }
    public double? CurrencyRate { get; set; }
    public short? SubToCurrencyTrans { get; set; }
    public double? CurrencyFixedRate { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    public virtual ICollection<CompanyBranch> CompanyBranches { get; set; } = new List<CompanyBranch>();
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
