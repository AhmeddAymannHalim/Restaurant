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
    public class SellTypeController : LookupCrudController<SellType>
    {
        public SellTypeController(ILookupCrudService<SellType> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.SellType";

        protected override string Icon => "fa-tags";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("Show", LookupFieldKind.Bool),
            new("IsLastPrice", LookupFieldKind.Bool),
            new("PlusPrice", LookupFieldKind.Decimal)
        };
    }
}
