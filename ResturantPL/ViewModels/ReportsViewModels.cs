using Restaurant.DAL.Models;

namespace ResturantPL.ViewModels
{
    public class ImportsViewModel
    {
        public DateTime PrintDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Total { get; set; }

    }
    public class OrdersByDatesViewModel : ImportsViewModel
    {
        public List<Order> Orders { get; set; }

    }
    
    public class ImportsByDatesViewModel : ImportsViewModel
    {
        public List<Deposit> Deposits { get; set; }

    }
    public class ExpensesByDatesViewModel : ImportsViewModel
    {
        public List<ExitExpense> Expenses { get; set; }

    }
    public class PaymentByDatesViewModel : ImportsViewModel
    {
        public List<PaymentInvoiceSupplier> Payments { get; set; }

    }
    
    public class SupliersByDatesViewModel : ImportsViewModel
    {
        public List<SuppliersTotalViewModel> Suppliers { get; set; }

    }
    
    public class ScroungeByDatesViewModel : ImportsViewModel
    {
        public List<Scrounge> Scrounges { get; set; }

    }
    
    public class SalaryByDatesViewModel : ImportsViewModel
    {
        public List<Salary> Salaries { get; set; }

    }
    public class InvoicesByDatesViewModel : ImportsViewModel
    {
        public List<Invoice> Invoices { get; set; }

    }
    public class TayarsByDatesViewModel : ImportsViewModel
    {
        public List<Order> Tayars { get; set; }

    }
}
