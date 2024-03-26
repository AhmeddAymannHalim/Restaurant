using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Formats.Asn1.AsnWriter;

namespace ResturantPL.Controllers
{
    public class StoreController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public StoreController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var stores = _unitOfWork.StoreRepository.GetAll(a => a.Include(a => a.Storekeeper));

            return View(stores);
        }


        public IActionResult Details(int id)
        {
            var store = _unitOfWork.StoreRepository.GetById(a => a.Id == id, new string[] { "Storekeeper", "Invoices", "StoreProducts" }, new string[] { "Invoices.Supplier", "StoreProducts.Product" });
            if (store != null)
            {
                return View(store);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult Create()
        {
            //ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FirstName");
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll()
                  .Where(a => a.UserType == "Store Keeper" || a.UserType == "امين مخزن" || a.UserType == "أمين مخزن"), "EmployeeId", "FullName");

            return View();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Store store)
        {
            try
            {
                ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll()
                    .Where(a => a.UserType == "Store Keeper" || a.UserType == "امين مخزن" ||a.UserType == "امين مخازن" ||a.UserType == "أمين مخازن" || a.UserType == "أمين مخزن"), "EmployeeId", "FullName");
                var products = _unitOfWork.ProductRepository.GetAll();

                if (ModelState.IsValid)
                {
                    await _unitOfWork.StoreRepository.AddAsync(store);
                    foreach (var item in products)
                    {
                        StoreProduct storeProduct = new StoreProduct()
                        {
                            ProductId = item.Id,
                            ProductName = item.Name,
                            StoreId = store.Id,
                            StoreName = store.Name,
                            Quantity = 0,
                            PriceForUnit = 0,
                        };
                        await _unitOfWork.StoreProductRepository.AddAsync(storeProduct);
                    }

                    return RedirectToAction("Index");
                }
                return View(store);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FirstName");

            var store = _unitOfWork.StoreRepository.GetById(a => a.Id == id, new string[] { "Storekeeper" });
            if (store != null)
            {
                return View(store);
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Inventory(int id,DateTime startDate,DateTime endDate)
        {
            var store = _unitOfWork.StoreRepository.GetById(a => a.Id == id, new string[] {  "StoreProducts" });
            decimal total = 0;
            foreach (var item in store.StoreProducts)
            {
                total+= item.Quantity*item.PriceForUnit;
            }
           ViewBag.Total=total;
            return View(store);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Store store)
        {
            try
            {
                ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FirstName");

                if (ModelState.IsValid)
                {
                    _unitOfWork.StoreRepository.Update(store);
                    var products = _unitOfWork.ProductRepository.GetAll();
                    foreach (var item in products)
                    {
                        StoreProduct storeProduct = _unitOfWork.StoreProductRepository.
                            GetById(a => a.ProductId == item.Id && a.StoreId == store.Id);
                        if (storeProduct != null)
                        {
                            storeProduct.StoreName = store.Name;
                            await _unitOfWork.StoreProductRepository.UpdateAsync(storeProduct);
                        }
                        else
                        {
                            StoreProduct newStoreProduct = new StoreProduct()
                            {
                                ProductId = item.Id,
                                ProductName = item.Name,
                                StoreId = store.Id,
                                StoreName = store.Name,
                                Quantity = 0,
                                PriceForUnit = 0,
                            };
                            await _unitOfWork.StoreProductRepository.AddAsync(storeProduct);
                        }
                    }

                    return RedirectToAction("Index");
                }
                return View(store);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.StoreRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.StoreRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }
    }
}