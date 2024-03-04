using System.ComponentModel.DataAnnotations;

namespace ResturantPL.ViewModels
{
	public class ForgetPasswordViewModel
	{
		[Required(ErrorMessage = "Email is required"), EmailAddress]
		public string Email { get; set; }
	}
}
