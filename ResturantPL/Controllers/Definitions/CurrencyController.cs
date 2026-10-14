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
    public class CurrencyController : LookupCrudController<Currency>
    {
        public CurrencyController(ILookupCrudService<Currency> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.Currency";

        protected override string Icon => "fa-money-bill";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("SubArName", LookupFieldKind.Text),
            new("SubEnName", LookupFieldKind.Text),
            new("CurrencyRate", LookupFieldKind.Decimal),
            new("SubToCurrencyTrans", LookupFieldKind.Int),
            new("CurrencyFixedRate", LookupFieldKind.Decimal)
        };
    }
}
