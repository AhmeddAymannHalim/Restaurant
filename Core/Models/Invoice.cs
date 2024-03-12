using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public enum InvoiceType
    {
        Purchases,
        Returns
    }


    public enum PayType
    {
        Cash,
        Later
    }
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public PayType PayType { get; set; }
        public InvoiceType InvoiceType { get; set; }


        public int SupplierId { get; set; }
        public int StoreId { get; set; }


        public virtual Supplier Supplier { get; set; }
        public virtual Store Store { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }

    }
}
