using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Restaurant.Data.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<ExitExpense> Expenses { get; set; }
    }
    
   
}


 