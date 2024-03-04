using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class State
    {
        public int StateId { get; set; }

        [DisplayName("اسم حالة المبلغ")]
        public string StateName { get; set; }    
    }
}
