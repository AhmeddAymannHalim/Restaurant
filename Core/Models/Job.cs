using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Job
    {
        public int JobId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }


        public virtual ICollection<Employee> Employees { get; set; }

    }
}
