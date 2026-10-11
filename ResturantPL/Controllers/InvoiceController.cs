using Microsoft.AspNetCore.Identity;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
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
                a => a.Include(a => a.Store));

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
                        //StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(a => a.Id == item.StoreProductId && a.StoreId == invoice.StoreId);
                        StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(a => a.StoreId == invoice.StoreId && a.ProductId == item.ProductId);

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


        public IActionResult UpdateInvoice(int id)
        {
            var invoice = _unitOfWork.InvoiceRepository.GetById(a => a.Id == id, new string[] { "InvoiceItems", "Supplier", "Store" }, new string[] { "InvoiceItems.StoreProduct" });

            if (invoice == null)
            {
                return NotFound();
            }

            ViewBag.Supplier = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");
            ViewBag.Store = new SelectList(_unitOfWork.StoreRepository.GetAll(), "Id", "Name");
            ViewBag.UntiPrice = new SelectList(_unitOfWork.UnitPriceRepository.GetAll(), "Id", "Name");
            ViewBag.Products = new SelectList(_unitOfWork.StoreProductRepository.GetAll().Where(a => a.StoreId == invoice.StoreId), "ProductId", "ProductName");
            ViewBag.AllTotalPrice = invoice.TotalPrice + invoice.Discount;
            ViewBag.Remain = invoice.Remaining;
            ViewBag.Balance = invoice.Supplier.Balance;

            var invoiceViewModel = new UpdateInvoiceViewModel()
            {
                Id = invoice.Id,
                InvoiceCode = invoice.InvoiceCode,
                InvoiceDate = invoice.InvoiceDate,
                Discount = invoice.Discount,
                TotalPrice = invoice.TotalPrice,
                Paid = invoice.Paid,
                Remaining = invoice.Remaining,
                InvoiceType = invoice.InvoiceType,
                PayType = invoice.PayType,
                SupplierId = invoice.SupplierId,
                StoreId = invoice.StoreId,
            };

            foreach (var item in invoice.InvoiceItems)
            {
                var product = _unitOfWork.StoreProductRepository.GetById(item.StoreProductId);
                var unit = _unitOfWork.UnitPriceRepository.GetById(item.UnitPriceId);
                UpdateInvoiceItemViewModel InvoiceItem = new UpdateInvoiceItemViewModel()
                {
                    InvoiceId = invoice.Id,
                    ProductName = product.ProductName,
                    ProductId = item.StoreProduct.ProductId,
                    UnitPriceId = item.UnitPriceId,
                    UnitPriceName = unit.ArName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Total
                };
                invoiceViewModel.InvoiceItems.Add(InvoiceItem);
            };
            return View(invoiceViewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInvoice(UpdateInvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lastInvoice = _unitOfWork.InvoiceRepository.GetById(a => a.Id == model.Id, new string[] { "InvoiceItems", "Supplier", "Store" }, new string[] { "InvoiceItems.StoreProduct" });

                if (lastInvoice == null)
                {
                    return NotFound();
                }
                if (model.InvoiceItems.Count() < 1)
                {
                    return RedirectToAction("UpdateInvoice", new { id = lastInvoice.Id });
                }

                // return the last values
                foreach (var item in lastInvoice.InvoiceItems)
                {
                    StoreProduct lastStoreProduct = _unitOfWork.StoreProductRepository.GetById(item.StoreProductId);

                    if (lastInvoice.InvoiceType == InvoiceType.Purchases)
                    {
                        lastStoreProduct.Quantity -= item.Quantity;
                        lastStoreProduct.PriceForUnit = item.Price;
                    }
                    else
                    {
                        lastStoreProduct.Quantity += item.Quantity;
                    }

                    await _unitOfWork.StoreProductRepository.UpdateAsync(lastStoreProduct);

                }
                foreach (var item in lastInvoice.InvoiceItems)
                {
                    _unitOfWork.InvoiceItemRepository.Delete(item);
                }


                // return the last values
                Supplier supplier = _unitOfWork.SupplierRepository.GetById(lastInvoice.SupplierId);

                if (lastInvoice.InvoiceType == InvoiceType.Purchases)
                {
                    supplier.Balance -= lastInvoice.Remaining;
                }
                else
                {
                    supplier.Balance += lastInvoice.Remaining;
                }

               await _unitOfWork.SupplierRepository.UpdateAsync(supplier);




                lastInvoice.InvoiceCode = model.InvoiceCode;
                lastInvoice.InvoiceDate = DateTime.Now;
                lastInvoice.StoreId = model.StoreId;
                lastInvoice.SupplierId = model.SupplierId;
                lastInvoice.InvoiceType = model.InvoiceType;
                lastInvoice.Discount = model.Discount;
                lastInvoice.TotalPrice = model.TotalPrice;
                lastInvoice.Paid = model.Paid;
                lastInvoice.Remaining = model.Remaining;
                lastInvoice.PayType = model.PayType;

                await _unitOfWork.InvoiceRepository.UpdateAsync(lastInvoice);

                foreach (var item in model.InvoiceItems)
                {
                    StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(a => a.StoreId == model.StoreId && a.ProductId == item.ProductId);
                    InvoiceItem invoiceItem = new InvoiceItem()
                    {
                        InvoiceId = model.Id,
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

                    await _unitOfWork.StoreProductRepository.UpdateAsync(storeProduct);
                }

                Supplier newSupplier = _unitOfWork.SupplierRepository.GetById(lastInvoice.SupplierId);

                if (lastInvoice.InvoiceType == InvoiceType.Purchases)
                {
                    newSupplier.Balance += lastInvoice.Remaining;
                }
                else
                {
                    newSupplier.Balance -= lastInvoice.Remaining;
                }

                await _unitOfWork.SupplierRepository.UpdateAsync(newSupplier);

                return RedirectToAction("DetailsInvoice", new { id = lastInvoice.Id });
            }
            var lastInvoice1 = _unitOfWork.InvoiceRepository.GetById(a => a.Id == model.Id, new string[] { "InvoiceItems", "Supplier", "Store" }, new string[] { "InvoiceItems.StoreProduct" });

            ViewBag.Supplier = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");
            ViewBag.Store = new SelectList(_unitOfWork.StoreRepository.GetAll(), "Id", "Name");
            ViewBag.UntiPrice = new SelectList(_unitOfWork.UnitPriceRepository.GetAll(), "Id", "Name");
            ViewBag.Products = new SelectList(_unitOfWork.StoreProductRepository.GetAll().Where(a => a.StoreId == lastInvoice1.StoreId), "ProductId", "ProductName");
            ViewBag.AllTotalPrice = lastInvoice1.TotalPrice + lastInvoice1.Discount;
            ViewBag.Remain = lastInvoice1.Remaining;
            return View(model);
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


        public IActionResult ProductQuantity(int productId, int storeId)
        {
            StoreProduct storeProduct = _unitOfWork.StoreProductRepository.GetById(a => a.ProductId == productId && a.StoreId == storeId);
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
                    Supplier supplier = _unitOfWork.SupplierRepository.GetById(obj.SupplierId);

                    if (obj.InvoiceType == InvoiceType.Purchases)
                    {
                        supplier.Balance -= obj.Remaining;
                    }
                    else
                    {
                        supplier.Balance += obj.Remaining;
                    }

                     _unitOfWork.SupplierRepository.Update(supplier);


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
