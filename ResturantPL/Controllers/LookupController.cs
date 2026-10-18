using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Commons;

namespace ResturantPL.Controllers
{
    [Authorize(Policy = "Staff")]
    public class LookupController : Controller
    {
        private readonly ILookupService _service;

        public LookupController(ILookupService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Branches()
        {
            return Json(await _service.BranchLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Jobs()
        {
            return Json(await _service.JobLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Employees()
        {
            return Json(await _service.EmployeeLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> DeliveryDrivers()
        {
            return Json(await _service.DeliveryDriverLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Storekeepers()
        {
            return Json(await _service.StorekeeperLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Kitchens()
        {
            return Json(await _service.KitchenLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> MenuSections()
        {
            return Json(await _service.MenuSectionLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ReadyMenuSections()
        {
            return Json(await _service.ReadyMenuSectionLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Tables()
        {
            return Json(await _service.TableLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Stores()
        {
            return Json(await _service.StoreLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            return Json(await _service.ProductLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ProductCategories()
        {
            return Json(await _service.ProductCategoryLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Units()
        {
            return Json(await _service.UnitPriceLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Suppliers()
        {
            return Json(await _service.SupplierLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            return Json(await _service.CustomerLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> StoreProducts(int storeId)
        {
            return Json(await _service.StoreProductLookupByStoreIdAsync(storeId));
        }

        [HttpGet]
        public async Task<IActionResult> Expenses()
        {
            return Json(await _service.ExpenseLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            return Json(await _service.RoleLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Countries()
        {
            return Json(await _service.CountryLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Governorates(int countryId)
        {
            return Json(await _service.GovernorateLookupAsync(countryId));
        }

        [HttpGet]
        public async Task<IActionResult> Cities(int governorateId)
        {
            return Json(await _service.CityLookupAsync(governorateId));
        }

        [HttpGet]
        public async Task<IActionResult> Villages(int townId)
        {
            return Json(await _service.VillageLookupAsync(townId));
        }
    }
}
