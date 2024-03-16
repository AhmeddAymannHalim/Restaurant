namespace ResturantPL.ViewModels
{
    public class SupplierBalanceViewModel
    {
        public decimal Deptor { get; set; }
        public decimal Creditor { get; set; }
        public DateTime Date { get; set; }
        public string Info { get; set; }
        public int InvoiceId { get; set; }

    }


    public class SupplierTotalViewModel
    {
        public List<SupplierBalanceViewModel> balances { get; set; }
        public decimal TotalDeptor { get; set; }
        public decimal TotalCreditor { get; set; }
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }


        public decimal LastTotalDeptor { get; set; }
        public decimal LastTotalCreditor { get; set; }
        public decimal LastTotal { get; set; }

    }
}
