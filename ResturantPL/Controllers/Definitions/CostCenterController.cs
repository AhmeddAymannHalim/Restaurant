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
    public class CostCenterController : LookupCrudController<CostCenter>
    {
        public CostCenterController(ILookupCrudService<CostCenter> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.CostCenter";

        protected override string Icon => "fa-bullseye";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("Remarks", LookupFieldKind.Text),
            new("ParentId", LookupFieldKind.Parent)
        };
    }
}
