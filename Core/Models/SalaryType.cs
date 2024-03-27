using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class SalaryType
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="نوع الراتب مطلوب")]
        public string Type { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
