using System;
using System.Collections.Generic;

namespace Restaurant.Data.Models;

public partial class SellType
{
    public int SellTypeId { get; set; }

    public string SellTypeArName { get; set; }

    public string SellTypeEnName { get; set; }

    public string ShowOrNot { get; set; }

    public bool? SellDefault { get; set; }

    public double? SellPlusPrice { get; set; }


}
