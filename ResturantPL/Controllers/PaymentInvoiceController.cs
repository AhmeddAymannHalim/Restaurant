using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class PaymentInvoiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentInvoiceController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }


        public IActionResult IndexSupplier()
        {
            var suppliers = _unitOfWork.PaymentInvoiceSupplierRepository.GetAll(a => a.Include(a => a.Supplier));
            return View(suppliers);
        }


        public IActionResult IndexCustomer()
        {
            var customer = _unitOfWork.PaymentInvoiceCustomerRepository.GetAll(a => a.Include(a => a.Customer));
            return View(customer);
        }


        public IActionResult SupplierInvoiceDetails(int id)
        {
            PaymentInvoiceSupplier supplier = _unitOfWork.PaymentInvoiceSupplierRepository.GetById(a => a.Id == id, new string[] { "Supplier" });
            if (supplier != null)
            {
                return View(supplier);
            }
            return NotFound();
        }


        public IActionResult CustomerInvoiceDetails(int id)
        {
            PaymentInvoiceCustomer customer = _unitOfWork.PaymentInvoiceCustomerRepository.GetById(a => a.Id == id, new string[] { "Customer" });
            if (customer != null)
            {
                return View(customer);
            }
            return NotFound();
        }

        public async Task<IActionResult> InvoicePayForSupplier()
        {
            ViewBag.Supplier = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");

            PaymentInvoiceSupplier lastInvoice = await _unitOfWork.PaymentInvoiceSupplierRepository.GetLast(item => item.Id);

            if (lastInvoice == null)
            {
                ViewBag.Invoice = 1;
            }
            else
            {
                int invoiceCode = int.Parse(lastInvoice.ProcessCode) + 1;
                ViewBag.Invoice = invoiceCode;
            }

            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InvoicePayForSupplier(PaymentInvoiceSupplier invoice)
        {
            try
            {
                ViewBag.Supplier = new SelectList(_unitOfWork.SupplierRepository.GetAll(), "Id", "Name");

                PaymentInvoiceSupplier lastInvoice = await _unitOfWork.PaymentInvoiceSupplierRepository.GetLast(item => item.Id);

                if (lastInvoice == null)
                {
                    ViewBag.Invoice = 1;
                }
                else
                {
                    int invoiceCode = int.Parse(lastInvoice.ProcessCode) + 1;
                    ViewBag.Invoice = invoiceCode;
                }
                if (ModelState.IsValid)
                {
                    invoice.Date = DateTime.Now;
                    await _unitOfWork.PaymentInvoiceSupplierRepository.AddAsync(invoice);

                    Supplier supplier = _unitOfWork.SupplierRepository.GetById(invoice.SupplierId);
                    if (supplier != null)
                    {
                        supplier.Balance -= invoice.Paid;
                        _unitOfWork.SupplierRepository.Update(supplier);
                    }
                    return RedirectToAction("SupplierInvoiceDetails", new { id = invoice.Id });
                }
                return View(invoice);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }
        public IActionResult SupplierInvoiceEdit(int id)
        {
            PaymentInvoiceSupplier invoice = _unitOfWork.PaymentInvoiceSupplierRepository.GetById(a => a.Id == id, new string[] { "Supplier" });

            if (invoice != null)
            {
                return View(invoice);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult SupplierInvoiceEdit(PaymentInvoiceSupplier invoice)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    PaymentInvoiceSupplier existingInvoice = _unitOfWork.PaymentInvoiceSupplierRepository.GetById(invoice.Id);

                    decimal lastPaid = existingInvoice.Paid;

                    existingInvoice.Description = invoice.Description;
                    existingInvoice.Date = DateTime.Now;
                    existingInvoice.Paid = invoice.Paid;

                    _unitOfWork.PaymentInvoiceSupplierRepository.Update(existingInvoice);

                    Supplier supplier = _unitOfWork.SupplierRepository.GetById(existingInvoice.SupplierId);
                    if (supplier != null)
                    {
                        supplier.Balance -= (invoice.Paid - lastPaid);
                        _unitOfWork.SupplierRepository.Update(supplier);
                    }

                    return RedirectToAction("SupplierInvoiceDetails", new { id = existingInvoice.Id });
                }
                return View(invoice);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        public IActionResult CustomerInvoiceEdit(int id)
        {
            PaymentInvoiceCustomer invoice = _unitOfWork.PaymentInvoiceCustomerRepository.GetById(a => a.Id == id, new string[] { "Customer" });

            if (invoice != null)
            {
                return View(invoice);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CustomerInvoiceEdit(PaymentInvoiceCustomer invoice)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    PaymentInvoiceCustomer existingInvoice = _unitOfWork.PaymentInvoiceCustomerRepository.GetById(invoice.Id);

                    decimal lastPaid = existingInvoice.Paid;

                    existingInvoice.Description = invoice.Description;
                    existingInvoice.Date = DateTime.Now;
                    existingInvoice.Paid = invoice.Paid;

                    _unitOfWork.PaymentInvoiceCustomerRepository.Update(existingInvoice);

                    Customer customer = _unitOfWork.CustomerRepository.GetById(existingInvoice.CustomerId);
                    if (customer != null)
                    {
                        customer.Balance -= (invoice.Paid - lastPaid);
                        _unitOfWork.CustomerRepository.Update(customer);
                    }

                    return RedirectToAction("CustomerInvoiceDetails", new { id = existingInvoice.Id });
                }
                return View(invoice);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public async Task<IActionResult> InvoicePayForCustomer()
        {
            ViewBag.Customer = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");


            PaymentInvoiceCustomer lastInvoice = await _unitOfWork.PaymentInvoiceCustomerRepository.GetLast(item => item.Id);

            if (lastInvoice == null)
            {
                ViewBag.Invoice = 1;
            }
            else
            {
                int invoiceCode = int.Parse(lastInvoice.ProcessCode) + 1;
                ViewBag.Invoice = invoiceCode;
            }

            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InvoicePayForCustomer(PaymentInvoiceCustomer invoice)
        {
            try
            {
                ViewBag.Customer = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");

                PaymentInvoiceCustomer lastInvoice = await _unitOfWork.PaymentInvoiceCustomerRepository.GetLast(item => item.Id);

                if (lastInvoice == null)
                {
                    ViewBag.Invoice = 1;
                }
                else
                {
                    int invoiceCode = int.Parse(lastInvoice.ProcessCode) + 1;
                    ViewBag.Invoice = invoiceCode;
                }
                if (ModelState.IsValid)
                {
                    invoice.Date = DateTime.Now;
                    await _unitOfWork.PaymentInvoiceCustomerRepository.AddAsync(invoice);

                    Customer customer = _unitOfWork.CustomerRepository.GetById(invoice.CustomerId);
                    if (customer != null)
                    {
                        customer.Balance -= invoice.Paid;
                        _unitOfWork.CustomerRepository.Update(customer);
                    }
                    return RedirectToAction("CustomerInvoiceDetails", new { id = invoice.Id });
                }

                return View(invoice);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult DeleteCustomerPayment(int id)
        {
            var obj = _unitOfWork.PaymentInvoiceCustomerRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.PaymentInvoiceCustomerRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }

        public IActionResult DeleteSupplierPayment(int id)
        {
            var obj = _unitOfWork.PaymentInvoiceSupplierRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.PaymentInvoiceSupplierRepository.Delete(obj);
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
