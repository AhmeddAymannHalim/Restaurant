using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ResturantPL.ViewModels
{
    public class SignInViewModel
    {
       

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; }
       

        [Required(ErrorMessage = "Password is required"), PasswordPropertyText]
        public string Password { get; set; }


        
        public bool RememberMe { get; set; }
    }
}
