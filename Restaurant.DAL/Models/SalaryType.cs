using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class SalaryType
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
