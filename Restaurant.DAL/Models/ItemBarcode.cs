using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Restaurant.DAL.Models;

public class ItemBarcode : BaseEntity
{
    public string Barcode { get; set; }
    public int? ItemId { get; set; }

    public virtual Item Item { get; set; }
}
