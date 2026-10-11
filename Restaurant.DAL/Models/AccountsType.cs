using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Data.Models;

public partial class AccountsType
{
    [Key]
    public byte AccTypeId { get; set; }
    public string AccTypeArName { get; set; } = null!;
    public string AccTypeEnName { get; set; } = null!;


    public virtual ICollection<Account> Accounts { get; set; }
}
