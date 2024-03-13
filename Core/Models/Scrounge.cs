//using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{

    public enum MoneyState
    {
        Incentive, Discount, Dept    }


    public class Scrounge 
    {
        public int Id { get; set; }

        [DisplayName("الوصف")]
        public string Info { get; set; }

        [DisplayName("التاريخ")]
        public DateTime Date { get; set; } = DateTime.Now;

        [DisplayName("المبلغ")]
        public decimal Amount { get; set; }

        [Required]
        [DisplayName("الموظف")]
        public int EmployeeId { get; set; }

        [Required]
        [DisplayName("حالة المبلغ")]
        public MoneyState State { get; set; }

        public string UserId { get; set; }




        public ApplicationUser User { get; set; }
        public Employee Employee { get; set; }

    }
}
