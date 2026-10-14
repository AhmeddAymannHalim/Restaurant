using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Helper;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;

namespace ResturantPL.Controllers.Definitions
{
    [Authorize(Policy = "Staff")]
    public class StockMovementController : Controller
    {
        private readonly IStockMovementService _service;
        private readonly ILookupService _lookup;

        public StockMovementController(IStockMovementService service, ILookupService lookup)
        {
            _service = service;
            _lookup = lookup;
        }

        public async Task<IActionResult> Index([FromQuery] PageRequest request, int? storeId, int? productId)
        {
            try
            {
                request ??= new PageRequest();

                var model = new StockMovementIndexViewModel
                {
                    Result = await _service.GetPagedAsync(request, storeId, productId),
                    StoreId = storeId,
                    ProductId = productId,
                    Stores = await _lookup.StoreLookupAsync(),
                    Products = await _lookup.ProductLookupAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }
    }
}
