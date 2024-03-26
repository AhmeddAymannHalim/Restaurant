using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.ProductRepository.GetAll());
        }


        public IActionResult Details(int id)
        {
            Product product = GetProduct(id);
            if (product != null)
            {
                return View(product);
            }
            return NotFound();
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.ProductRepository.AddAsync(model);

                    var stores = _unitOfWork.StoreRepository.GetAll();
                    foreach (var item in stores)
                    {
                        StoreProduct storeProduct = new StoreProduct()
                        {
                            ProductId = model.Id,
                            ProductName = model.Name,
                            StoreId = item.Id,
                            StoreName = item.Name,
                            Quantity = 0,
                            PriceForUnit = 0,
                        };
                        await _unitOfWork.StoreProductRepository.AddAsync(storeProduct);
                    }

                    return RedirectToAction("Index");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Edit(int id)
        {
            Product product = GetProduct(id);
            if (product != null)
            {
                return View(product);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.ProductRepository.Update(model);

                    var stores = _unitOfWork.StoreRepository.GetAll();
                    foreach (var item in stores)
                    {
                        StoreProduct storeProduct = _unitOfWork.StoreProductRepository.
                            GetById(a => a.ProductId == model.Id && a.StoreId == item.Id);
                        if (storeProduct != null)
                        {
                            storeProduct.ProductName = model.Name;
                            await _unitOfWork.StoreProductRepository.UpdateAsync(storeProduct);
                        }
                        else
                        {
                            StoreProduct newStoreProduct = new StoreProduct()
                            {
                                ProductId = model.Id,
                                ProductName = model.Name,
                                StoreId = item.Id,
                                StoreName = item.Name,
                                Quantity = 0,
                                PriceForUnit = 0,
                            };
                            await _unitOfWork.StoreProductRepository.AddAsync(storeProduct);
                        }
                    }



                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.ProductRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.ProductRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }


        public Product GetProduct(int id)
        {
            Product product = _unitOfWork.ProductRepository.GetById(a => a.Id == id, new string[] { "InvoiceItems", "StoreProducts" }, new string[] { "InvoiceItems.Invoice", "StoreProducts.Store" });
            return product;
        }



    }
}
