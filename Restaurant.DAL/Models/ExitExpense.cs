using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class ExitExpense
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int ExpenseId { get; set; }
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
        public virtual Expense Expense { get; set; }
    }
}
