using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ResturantPL.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = "Password is required"), PasswordPropertyText, MinLength(6, ErrorMessage = "min charecters is 6")]
		public string Password { get; set; }

		[Required(ErrorMessage = "required"), Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }

		
		public string Email { get; set; }

		public string Token { get; set; }


	}
}
