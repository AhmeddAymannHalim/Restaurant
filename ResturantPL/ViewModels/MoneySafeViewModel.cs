namespace ResturantPL.ViewModels
{
    public class MoneySafeViewModel
    {
        public decimal Imports { get; set; }
        public decimal Sales { get; set; }
        public decimal Returns { get; set; }
        public decimal Purchases { get; set; }
        public decimal Expenses { get; set; }
        public decimal EmployeeExpenses { get; set; }
        public decimal SupplierPayment { get; set; }

        public decimal TotalExports { get; set; }
        public decimal TotalImports { get; set; }
        public decimal TotalMoney { get; set; }



        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public DateTime ReportDate { get; set; } = DateTime.Now;



    }
}
