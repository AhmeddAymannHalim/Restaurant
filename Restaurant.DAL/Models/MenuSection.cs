using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class MenuSection : BaseEntity
    {
        public string Code { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public int? KitchenId { get; set; } = null;
        public Kitchen Kitchen { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }
}
