using Shared.ViewModels.Commons;
using Shared.Enums;

namespace Shared.ViewModels.Definitions;

public class InvoiceViewModel
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    public InvoiceType InvoiceType { get; set; }
    public PayType PayType { get; set; } = PayType.Cash;
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public int StoreId { get; set; }
    public decimal Discount { get; set; }
    public decimal Paid { get; set; }
    public bool AutoPost { get; set; } = true;
    public bool AutoStock { get; set; } = true;
    public List<PostingDefaultViewModel> PostingDefaults { get; set; } = new();

    public List<InvoiceItemViewModel> Items { get; set; } = new();

    public bool IsSupplierInvoice => InvoiceType is InvoiceType.Purchases or InvoiceType.Returns;

    public decimal SubTotal => Items.Sum(a => a.Total);
    public decimal TotalPrice => SubTotal - Discount;
    public decimal Remaining => TotalPrice - Paid;

    public string SupplierArName { get; set; }
    public string SupplierEnName { get; set; }
    public string CustomerArName { get; set; }
    public string CustomerEnName { get; set; }
    public string StoreArName { get; set; }
    public string StoreEnName { get; set; }

    public IEnumerable<LookupItem> Suppliers { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Customers { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Stores { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Units { get; set; } = new List<LookupItem>();
    public IEnumerable<LookupItem> Categories { get; set; } = new List<LookupItem>();
}

public class PostingDefaultViewModel
{
    public InvoiceType Type { get; set; }
    public bool AutoPost { get; set; }
    public bool AutoStock { get; set; }
}

public class InvoiceItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int UnitPriceId { get; set; }
    public string UnitName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;
}
