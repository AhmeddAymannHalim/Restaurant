using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    public class DepartmentController : LookupCrudController<Department>
    {
        public DepartmentController(ILookupCrudService<Department> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.Department";

        protected override string Icon => "fa-building";
    }
}
