using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public IActionResult Invoices()
        {
            var invoices = _unitOfWork.InvoiceRepository.GetAll(
                a => a.Include(a => a.InvoiceItems).ThenInclude(a => a.StoreProduct),
                a => a.Include(a => a.Supplier),
                a => a.Include(a => a.Store)
                );

            return View(invoices);
        }



        public IActionResult DetailsInvoice(int id)
        {
            Invoice invoice = _unitOfWork.InvoiceRepository.
                GetById(a => a.Id == id, new string[] { "Supplier", "InvoiceItems", "Store" }
                , new string[] { "InvoiceItems.StoreProduct", "InvoiceItems.UnitPrice" });
            if (invoice != null)
            {
                return View(invoice);
            }
            return NotFound();
        }
      
        public async Task<IActionResult> OpenInvoice()
        {
            ViewBag.Supplier = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");
            ViewBag.Store = new SelectList(_unitOfWork.StoreRepository.GetAll(), "Id", "Name");
            ViewBag.UntiPrice = new SelectList(_unitOfWork.UnitPriceRepository.GetAll(), "Id", "Name");


            Invoice lastInvoice = await _unitOfWork.InvoiceRepository.GetLast(item => item.Id);

            if (lastInvoice == null)
            {
                ViewBag.Invoice = 1;
            }
            else
            {
                int invoiceCode = int.Parse(lastInvoice.InvoiceCode) + 1;
                ViewBag.Invoice = invoiceCode;
            }

            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenInvoice(InvoiceViewModel model)
        {
            try
            {
                if (model.InvoiceItems.Count() < 1)
                {
                    return RedirectToAction("OpenInvoice");
                }
                if (ModelState.IsValid)
                {
                    Invoice invoice = new Invoice()
                    {
                        InvoiceCode = model.InvoiceCode,
                        InvoiceDate = DateTime.Now,
                        StoreId = model.StoreId,
                        SupplierId = model.SupplierId,
                        InvoiceType = model.InvoiceType,
                        Discount = model.Discount,
                        TotalPrice = model.TotalPrice,
                        Paid = model.Paid,
                        Remaining = model.Remaining,
                        PayType = model.PayType,
                    };

                    await _unitOfWork.InvoiceRepository.AddAsync(invoice);
                    foreach (var item in model.InvoiceItems)
                    {
                        StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(a => a.Id == item.StoreProductId && a.StoreId == invoice.StoreId);

                        InvoiceItem invoiceItem = new InvoiceItem()
                        {
                            InvoiceId = invoice.Id,
                            StoreProductId = storeProduct.Id,
                            UnitPriceId = item.UnitPriceId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Total = item.Total
                        };
                        await _unitOfWork.InvoiceItemRepository.AddAsync(invoiceItem);

                        if (model.InvoiceType == InvoiceType.Purchases)
                        {
                            storeProduct.Quantity += invoiceItem.Quantity;
                            storeProduct.PriceForUnit = invoiceItem.Price;
                        }
                        else
                        {
                            storeProduct.Quantity -= invoiceItem.Quantity;
                        }
                        _unitOfWork.StoreProductRepository.Update(storeProduct);
                    }


                    Supplier supplier = _unitOfWork.SupplierRepository.GetById(invoice.SupplierId);

                    if (invoice.InvoiceType == InvoiceType.Purchases)
                    {
                        supplier.Balance += invoice.Remaining;
                    }
                    else
                    {
                        supplier.Balance -= invoice.Remaining;
                    }

                    _unitOfWork.SupplierRepository.Update(supplier);

                    return RedirectToAction("DetailsInvoice", new { id = invoice.Id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult SupplierBalance(int supplierId)
        {
            Supplier supplier = _unitOfWork.SupplierRepository.GetById(supplierId);
            if (supplier != null)
            {

                return Json(supplier.Balance);
            }
            return BadRequest();
        }


        public IActionResult ProductQuantity(int storeProductId)
        {
            StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(storeProductId);
            if (storeProduct != null)
            {
                return Json(storeProduct.Quantity);
            }

            return BadRequest();
        }


        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.InvoiceRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.InvoiceRepository.Delete(obj);
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
