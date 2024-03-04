using Restaurant.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ResturantPL.ViewModels
{
    public class BranchViewModel
    {
        public int Id { get; set; }
        [Required, Display(Name ="الاسم")]
        public string Name { get; set; }
        [Required, Display(Name = "العنوان")]
        public string Address { get; set; }
        [Required, Display(Name = "الكود")]
        public string Code { get; set; }
        [Required, MaxLength(11), Display(Name = "رقم الهاتف")]
        public string Phone1 { get; set; }
        [Required, MaxLength(11), Display(Name = "رقم الهاتف 2")]
        public string Phone2 { get; set; }
        [Required, MaxLength(11), Display(Name = "رقم الموبايل")]
        public string Mobile1 { get; set; }
        [Required, MaxLength(11), Display(Name = "رقم الموبايل 2")]
        public string Mobile2 { get; set; }

        public List<Employee> EmployeeList { get; set; } = new List<Employee>();
        public List<ApplicationUser> UserList { get; set; } = new List<ApplicationUser>();

    }
}
