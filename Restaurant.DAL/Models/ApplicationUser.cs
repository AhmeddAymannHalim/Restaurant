using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public int? TownId { get; set; }
        public string Street { get; set; }
        public string Image { get; set; }
        public byte Age { get; set; } = 0;
        public string UserType { get; set; }
        public string Code { get; set; }

        // not stored: the employee a login form is opened for, before the login exists
        public int? EmployeeId { get; set; }

        public virtual Town Town { get; set; }
    }
}
