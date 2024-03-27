using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{

    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }


        [Display(Name ="الاسم الاول"),Required(ErrorMessage = "الاسم الاول مطلوب"), MaxLength(10, ErrorMessage = "max charactes is 10"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string FirstName { get; set; }


        [Display(Name = "الاسم الاخير"), Required(ErrorMessage = "الاسم الاخير مطلوب"), MaxLength(10, ErrorMessage = "max charactes is 10"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string LastName { get; set; }


        [Display(Name = "رقم الهاتف"), StringLength(11, MinimumLength = 11)]
        public string PhoneNumber { get; set; }


        [Display(Name = "رقم الموبايل"), Required(ErrorMessage = "رقم الموبايل مطلوب"), StringLength(11, MinimumLength = 11)]
        public string MobileNumber { get; set; }


        [Display(Name = "الدولة"), Required(ErrorMessage = "اسم الدولة مطلوب"), MaxLength(30, ErrorMessage = "max charactes is 30"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string Country { get; set; }


        [Display(Name = "المدينة"), Required(ErrorMessage = "اسم المدينة مطلوب"), MaxLength(20, ErrorMessage = "max charactes is 20"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string City { get; set; }


        [Display(Name = "الشارع"), Required(ErrorMessage = "اسم الشارع مطلوب"), MaxLength(100, ErrorMessage = "max charactes is 100"), MinLength(3, ErrorMessage = "min charecters is 3")]
        public string Street { get; set; }


        [Display(Name ="الصورة")]
        public IFormFile ImageUrl { get; set; } = null;
        public string Image { get; set; } = null;


        [Display(Name = "السن"), Required(ErrorMessage = "العمر مطلوب")]
        public byte Age { get; set; }


        [Display(Name = "كود الموظف"), Required(ErrorMessage = "الكود مطلوب")]
        public string Code { get; set; }


        [Display(Name = "الرقم القومي"), Required(ErrorMessage = "الرقم القومي مطلوب"), StringLength(14, MinimumLength = 14)]
        public string NationalID { get; set; }


        [Display(Name = "نوع الراتب"), Required(ErrorMessage = "نوع الراتب مطلوب")]
        public int SalaryTypeId { get; set; }


        [Display(Name = "الراتب"), Required(ErrorMessage = "مبلغ الراتب مطلوب")]
        public decimal Salary { get; set; }


        [Display(Name = "الفرع"), Required(ErrorMessage = "اسم الفرع مطلوب")]
        public int BrancheId { get; set; }

        
        public string UserType { get; set; }


        [Display(Name = "الوظيفة"), Required(ErrorMessage = "اسم الوظيفة مطلوب")]
        public int JobId { get; set; }

    }
}
