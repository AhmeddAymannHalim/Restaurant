//using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{

public class Scrounge 
    {
        public int Id { get; set; }

        public string Info { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public decimal Amount { get; set; }

        public int EmployeeId { get; set; }

        public MoneyState State { get; set; }

        public string UserId { get; set; }




        public ApplicationUser User { get; set; }
        public Employee Employee { get; set; }

    }
}
