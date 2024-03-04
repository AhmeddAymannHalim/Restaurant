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



    }
}
