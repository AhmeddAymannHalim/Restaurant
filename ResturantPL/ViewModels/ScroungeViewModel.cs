using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;

namespace ResturantPL.ViewModels
{
    public class ScroungeViewModel
    {
        public Scrounge Scrounge { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> EmployeeList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> StateList { get; set; } 
    }
}
