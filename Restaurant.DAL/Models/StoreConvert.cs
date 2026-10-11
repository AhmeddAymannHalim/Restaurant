using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class StoreConvert
    {
        public int Id { get; set; }
        public string ConvertCode { get; set; }
        public DateTime ConvertDate { get; set; }
        public int FromStoreId { get; set; }
        public int ToStoreId { get; set; }


        public virtual Store FromStore { get; set; }
        public virtual Store ToStore { get; set; }
        public virtual ICollection<StoreConvertProduct> StoreConvertProducts { get; set; }

    }
}
