using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Data.Models
{
    public partial class AccountState
    {
        public byte Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
