using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class MenuSection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? kitchenId { get; set; } = null;
        public kitchen kitchen { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }
}
