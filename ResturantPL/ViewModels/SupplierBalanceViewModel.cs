namespace ResturantPL.ViewModels
{
    public class SupplierBalanceViewModel
    {
        public decimal Deptor { get; set; }
        public decimal Creditor { get; set; }
        public decimal Balance { get; set; }
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
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }


        public decimal LastTotalDeptor { get; set; }
        public decimal LastTotalCreditor { get; set; }
        public decimal LastTotal { get; set; }

        public decimal AllTotalDeptor { get; set; }
        public decimal AllTotalCreditor { get; set; }
        public decimal Balance { get; set; }

    }


    public class SuppliersTotalViewModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public decimal TotalDeptor { get; set; }
        public decimal TotalCreditor { get; set; }
        public decimal Total { get; set; }


        public decimal LastTotalDeptor { get; set; }
        public decimal LastTotalCreditor { get; set; }
        public decimal LastTotal { get; set; }
        public decimal AllTotal { get; set; }

    }
}
