using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Data.Models;

public partial class PayType
{
    public int PayTypeId { get; set; }
    public string PayTypeCode { get; set; }
    public string PayTypeArName { get; set; }
    public string PayTypeEnName { get; set; }
    public string PayTypeNotes { get; set; }
}
