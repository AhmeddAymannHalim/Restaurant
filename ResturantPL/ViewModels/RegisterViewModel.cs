using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ResturantPL.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="First name is required"), MaxLength(10,ErrorMessage ="max charactes is 10") , MinLength(3,ErrorMessage ="min charecters is 3")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required"), MaxLength(10, ErrorMessage = "max charactes is 10"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Password is required"),PasswordPropertyText, MinLength(6, ErrorMessage = "min charecters is 6")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required"), StringLength(11,MinimumLength =11)]
        public string PhoneNumber { get; set; }

   
        

        [Required(ErrorMessage = "required")]
        public bool IsAgree { get; set; }

		[Required(ErrorMessage = "required") , Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }

	}
}
