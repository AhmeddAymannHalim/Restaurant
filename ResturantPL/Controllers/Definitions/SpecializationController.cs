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
    public class SpecializationController : LookupCrudController<Specialization>
    {
        public SpecializationController(ILookupCrudService<Specialization> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.Specialization";

        protected override string Icon => "fa-user-graduate";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("Type", LookupFieldKind.Enum, typeof(MainOrBranchType)),
            new("ParentId", LookupFieldKind.Parent)
        };
    }
}
