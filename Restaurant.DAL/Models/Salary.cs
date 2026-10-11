using System.Collections.Generic;
using System.Threading.Tasks; 
using System.Runtime;     
using System.Linq;    
using System.Text;
using System; 
          
      
  
 
namespace Restaurant.DAL.Models
{
    public class Salary
    {
        public int Id { get; set; }
        public string SalaryCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime SalaryDate { get; set; }


        public decimal SalaryAmount { get; set; }
        public int EmployeeId { get; set; }
        public string UserId { get; set; }


        public ApplicationUser User { get; set; }
        public Employee Employee { get; set; }
    }
}
