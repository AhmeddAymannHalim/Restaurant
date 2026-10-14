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
    public class CompanyController : LookupCrudController<Company>
    {
        private readonly ILookupService _lookup;

        public CompanyController(ILookupCrudService<Company> service, ILookupService lookup, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
            _lookup = lookup;
        }

        protected override string TitleKey => "Lookup.Company";

        protected override string Icon => "fa-city";

        protected override IReadOnlyList<LookupField> Fields { get; } = new LookupField[]
        {
            new("CurrencyId", LookupFieldKind.Reference)
        };

        protected override async Task LoadReferenceOptionsAsync(LookupEditViewModel model)
        {
            model.Options["CurrencyId"] = await _lookup.NamedLookupAsync<Currency>();
        }
    }
}
