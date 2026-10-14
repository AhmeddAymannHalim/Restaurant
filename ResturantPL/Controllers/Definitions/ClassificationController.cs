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
    public class ClassificationController : LookupCrudController<Classification>
    {
        public ClassificationController(ILookupCrudService<Classification> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.Classification";

        protected override string Icon => "fa-layer-group";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("Description", LookupFieldKind.Text)
        };
    }
}
