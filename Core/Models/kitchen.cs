using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class kitchen
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="هذا الحقل مطلوب")]
        [MaxLength(100,ErrorMessage ="يجب ان لا يزيد الاسم عن 100 حرف")]
        public string Name { get; set; }
        public virtual ICollection<MenuSection> MenuSections { get; set; }
    }
}
