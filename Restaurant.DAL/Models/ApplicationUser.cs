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
        // Email , UserName, Password , ConfirmPassword
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Image { get; set; }
        public byte Age { get; set; } = 0;
        public string MobileNumber { get; set; }
        public string UserType { get; set; }
        public string Code { get; set; }


    }
}
