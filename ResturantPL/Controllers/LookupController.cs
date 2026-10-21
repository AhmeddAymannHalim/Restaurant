using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.Models;
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
            return Json(await _service.NamedLookupAsync<CompanyBranch>());
        }

        [HttpGet]
        public async Task<IActionResult> Jobs()
        {
            return Json(await _service.NamedLookupAsync<Job>());
        }

        [HttpGet]
        public async Task<IActionResult> Employees()
        {
            return Json(await _service.NamedLookupAsync<Employee>());
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
            return Json(await _service.NamedLookupAsync<Kitchen>());
        }

        [HttpGet]
        public async Task<IActionResult> MenuSections()
        {
            return Json(await _service.NamedLookupAsync<MenuSection>());
        }

        [HttpGet]
        public async Task<IActionResult> ReadyMenuSections()
        {
            return Json(await _service.NamedLookupAsync<MenuSection>());
        }

        [HttpGet]
        public async Task<IActionResult> Tables()
        {
            return Json(await _service.NamedLookupAsync<Restaurant.DAL.Models.Table>());
        }

        [HttpGet]
        public async Task<IActionResult> Stores()
        {
            return Json(await _service.NamedLookupAsync<Store>());
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            return Json(await _service.NamedLookupAsync<Product>());
        }

        [HttpGet]
        public async Task<IActionResult> ProductCategories()
        {
            return Json(await _service.NamedLookupAsync<ProductCategory>());
        }

        [HttpGet]
        public async Task<IActionResult> Units()
        {
            return Json(await _service.NamedLookupAsync<UnitPrice>());
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
            return Json(await _service.NamedLookupAsync<Expense>());
        }

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            return Json(await _service.RoleLookupAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Countries()
        {
            return Json(await _service.NamedLookupAsync<Country>());
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
