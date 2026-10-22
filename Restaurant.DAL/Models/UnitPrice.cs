using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class UnitPrice : BaseEntity
    {
        public string Code { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public decimal? Value { get; set; }
    }
}
