using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الطاولة مطلوب")]
        public string Name { get; set; }

        [Required(ErrorMessage = "عدد الكراسي مطلوب")]
        public byte NumOfPersons { get; set; }
        public bool IsPending { get; set; }
    }
}
