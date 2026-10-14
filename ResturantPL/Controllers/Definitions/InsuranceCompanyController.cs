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
    public class InsuranceCompanyController : LookupCrudController<InsuranceCompany>
    {
        public InsuranceCompanyController(ILookupCrudService<InsuranceCompany> service, IToastrService toastr, IStringLocalizer<SharedResource> localizer)
            : base(service, toastr, localizer)
        {
        }

        protected override string TitleKey => "Lookup.InsuranceCompany";

        protected override string Icon => "fa-building-shield";
    }
}
