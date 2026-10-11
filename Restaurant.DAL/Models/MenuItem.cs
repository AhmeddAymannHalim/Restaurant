using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Restaurant.DAL.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public int MenuSectionId { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int StoreId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Price { get; set; }
        public bool IsReady { get; set; }
        public decimal Benefit { get; set; }
        public Size? Size { get; set; }
        public virtual Store Store { get; set; }
        public virtual MenuSection MenuSection { get; set; }
        public virtual ICollection<MenuItemComponent> Components { get; set; }
        public virtual ICollection<OnlineOrderMenuItem> OnlineOrderMenuItems { get; set; }
        public virtual ICollection<OrderMenuItem> OrderMenuItems { get; set; }

    }
}
