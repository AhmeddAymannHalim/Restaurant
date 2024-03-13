using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{

    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }


        [Display(Name ="الاسم الاول"),Required(ErrorMessage = "First name is required"), MaxLength(10, ErrorMessage = "max charactes is 10"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string FirstName { get; set; }


        [Display(Name = "الاسم الاخير"), Required(ErrorMessage = "Last name is required"), MaxLength(10, ErrorMessage = "max charactes is 10"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string LastName { get; set; }


        [Display(Name = "رقم الهاتف"), Required(ErrorMessage = "Phone number is required"), StringLength(11, MinimumLength = 11)]
        public string PhoneNumber { get; set; }


        [Display(Name = "رقم الموبايل"), Required(ErrorMessage = "Phone number is required"), StringLength(11, MinimumLength = 11)]
        public string MobileNumber { get; set; }


        [Display(Name = "الدولة"), Required(ErrorMessage = "Country is required"), MaxLength(30, ErrorMessage = "max charactes is 30"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string Country { get; set; }


        [Display(Name = "المدينة"), Required(ErrorMessage = "City is required"), MaxLength(20, ErrorMessage = "max charactes is 20"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string City { get; set; }


        [Display(Name = "الشارع"), Required(ErrorMessage = "Street is required"), MaxLength(100, ErrorMessage = "max charactes is 100"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string Street { get; set; }


        [Display(Name ="الصورة")]
        public IFormFile ImageUrl { get; set; } = null;
        public string Image { get; set; } = null;


        [Display(Name = "السن"), Required(ErrorMessage = "Age is required")]
        public byte Age { get; set; }


        [Display(Name = "كود الموظف"), Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }


        [Display(Name = "الرقم القومي"), Required(ErrorMessage = "National ID is required"), StringLength(14, MinimumLength = 14)]
        public string NationalID { get; set; }


        [Display(Name = "نوع الراتب"), Required(ErrorMessage = "Salary Type is required")]
        public int SalaryTypeId { get; set; }


        [Display(Name = "الراتب"), Required(ErrorMessage = "Salary is required")]
        public decimal Salary { get; set; }


        [Display(Name = "الفرع"), Required(ErrorMessage = "Branche Name is required")]
        public int BrancheId { get; set; }

        
        public string UserType { get; set; }


        [Display(Name = "الوظيفة"), Required(ErrorMessage = "Job Title is required")]
        public int JobId { get; set; }


        //public virtual Branche Branche { get; set; } = new Branche();
        //public virtual SalaryType SalaryType { get; set; } = new SalaryType();
        //public virtual Job Job { get; set; } = new Job();
    }
}
