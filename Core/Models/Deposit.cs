using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public class Deposit
    {
        public int Id { get; set; }

        [DisplayName("كود العملية")]
        public string ProcessCode { get; set; }

        [DisplayName("اسم المودع")]
        public string DeposerName { get; set; }

        [DisplayName("المبلغ")]
        public decimal Amount { get; set; }

        [DisplayName("التاريخ")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [DisplayName("الوصف")]
        public string Description { get; set; }

        public bool IsDeleted { get; set; }


        [DisplayName("المستخدم")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }



    }
}
