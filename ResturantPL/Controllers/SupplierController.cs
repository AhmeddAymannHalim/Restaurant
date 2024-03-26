using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class SupplierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var supplier = _unitOfWork.SupplierRepository.GetAll(a => a.Include(a => a.Invoices));

            return View(supplier);
        }



        public IActionResult Details(int id)
        {
            Supplier supplier = _unitOfWork.SupplierRepository.GetById(a => a.Id == id, new string[] { "Invoices", "PaymentInvoices" }, new string[] { "Invoices.InvoiceItems" });
            if (supplier != null)
            {
                return View(supplier);
            }
            return NotFound();
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    supplier.Balance = 0;
                    await _unitOfWork.SupplierRepository.AddAsync(supplier);

                    return RedirectToAction("Index");
                }
                return View(supplier);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Supplier supplier = _unitOfWork.SupplierRepository.GetById(id);
            if (supplier != null)
            {
                return View(supplier);
            }
            return NotFound();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.SupplierRepository.Update(supplier);

                    return RedirectToAction("Index");
                }
                return View(supplier);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.SupplierRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.SupplierRepository.Delete(obj);
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
