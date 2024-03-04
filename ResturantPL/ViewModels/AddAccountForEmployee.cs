using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ResturantPL.ViewModels
{
    public class AddAccountForEmployee
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required"), PasswordPropertyText, MinLength(6, ErrorMessage = "min charecters is 6")]
        public string Password { get; set; }


        [Required]
        //[DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public int EmployeeId { get; set; }

    }
}
