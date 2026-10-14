using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Definitions;
using Restaurant.DAL.Models;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.Helpers.Localization;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    public class LeaveTypeController : LookupCrudController<LeaveType>
    {
        public LeaveTypeController(ILookupCrudService<LeaveType> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.LeaveType";

        protected override string Icon => "fa-umbrella-beach";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("DaysPerYear", LookupFieldKind.Int)
        };
    }
}
