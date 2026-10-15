
using Shared.Enums;
using Shared.Enums.Others;

namespace Restaurant.DAL.Models;

public class AccountSetting : BaseEntity
{
    public int? ParentCustomerId { get; set; }    
    public int? ParentSupplierId { get; set; }  
    public int? ParentExpensesId { get; set; }  
    public int? ParentBankId { get; set; }
    public int? ParentBoxId { get; set; }
    public int? ParentFixedAssetsId { get; set; }
    public int? ParentEmployeeId { get; set; }


    public virtual Account ParentCustomer { get; set; }
    public virtual Account ParentSupplier { get; set; }
    public virtual Account ParentExpenses { get; set; }
    public virtual Account ParentBank { get; set; }
    public virtual Account ParentBox { get; set; }
    public virtual Account ParentFixedAssets { get; set; }
    public virtual Account ParentEmployee { get; set; }

}
