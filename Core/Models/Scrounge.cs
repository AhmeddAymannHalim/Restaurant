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

        [Required]
        [DisplayName("الموظف")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        [DisplayName("حالة المبلغ")]
        public int StateId { get; set; }

        public string UserId { get; set; }




        public State State { get; set; }
        public ApplicationUser User { get; set; }

    }
}
