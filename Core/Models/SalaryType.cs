using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class SalaryType
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
