using System;
using System.Collections.Generic;

namespace Restaurant.DAL.Models;

public partial class AccountsType
{
    public byte AccTypeId { get; set; }
    public string AccTypeArName { get; set; } = null!;
    public string AccTypeEnName { get; set; } = null!;


    public virtual ICollection<Account> Accounts { get; set; }
}
