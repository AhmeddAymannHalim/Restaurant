using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class Deposit
    {
        public int Id { get; set; }

        public string ProcessCode { get; set; }

        public string DeposerName { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public string Description { get; set; }

        public bool IsDeleted { get; set; }


        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }



    }
}
