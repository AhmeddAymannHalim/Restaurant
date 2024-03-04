using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{
    public class InvoiceViewModel
    {

        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }

        public InvoiceType InvoiceType { get; set; }
        public PayType PayType { get; set; }
        public int SupplierId { get; set; }
        public int StoreId { get; set; }

        public List<InvoiceItemViewModel> InvoiceItems { get; set; }


    }
    public class InvoiceItemViewModel
    {
        public int StoreProductId { get; set; }
        public int UnitPriceId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
