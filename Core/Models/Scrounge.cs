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
    public class Scrounge 
    {
        public int Id { get; set; }
        [DisplayName("الوصف")]
        public string Info { get; set; }
        [DisplayName("التاريخ")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        [DisplayName("المبلغ")]
        public decimal Amount { get; set; }

        // the relation with Employee table
        [Required]
        //[DisplayName("Employee")]
        [DisplayName("الموظف")]
        public int EmployeeId { get; set; }
        //[ValidateNever]
        public Employee Employee { get; set; }

        // the relation with State table
        [Required]
        //[DisplayName("State")]
        [DisplayName("حالة المبلغ")]
        public int StateId { get; set; }
        //[ValidateNever]
        public State State { get; set; }

    }
}
