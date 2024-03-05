using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using ResturantPL.ViewModels;
using Rotativa.AspNetCore;
using Service.UnitOfWork;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class ReportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            this._userManager = userManager;
            this._unitOfWork = unitOfWork;
        }

        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }


        // Export Order to pdf
        public IActionResult OrderReport(int orderId)
        {
            var order = _unitOfWork.OrderRepository.GetById(a => a.Id == orderId,
                new string[] { "Seller", "Receiver", "Table", "Customer", "Employee", "OrderMenuItems" },
                new string[] { "OrderMenuItems.MenuItem" });


            // to open pdf and download if you want
            //return new ViewAsPdf( order);

            var viewAsPdf = new ViewAsPdf
            {
                Model = order,
                //ViewName = "OrderReport",
                FileName = $"{order.CustomerName}.pdf"
            };
            return viewAsPdf;
        }

        // Export Invoice to pdf
        public IActionResult InvoiceReport(int invoiceId)
        {
            Invoice invoice = _unitOfWork.InvoiceRepository.
                 GetById(a => a.Id == invoiceId, new string[] { "Supplier", "InvoiceItems", "Store" }
                 , new string[] { "InvoiceItems.StoreProduct", "InvoiceItems.UnitPrice" });

            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة رقم {invoice.InvoiceCode}.pdf"
            };
            return viewAsPdf;
        }

        // Report for convert product between stores invoice 
        public IActionResult StoreConvertReport(int invoiceId)
        {
            StoreConvert invoice = _unitOfWork.StoreConvertRepository.
                 GetById(a => a.Id == invoiceId, new string[] { "FromStore", "ToStore", "StoreConvertProducts" }, new string[] { "StoreConvertProducts.StoreProduct" });

            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة نقل اصناف رقم {invoice.ConvertCode}.pdf"
            };
            return viewAsPdf;
        }

        // Report for  payment invoice for supplier
        public IActionResult PayForSuppliertReport(int invoiceId)
        {
            PaymentInvoiceSupplier invoice = _unitOfWork.PaymentInvoiceSupplierRepository.
                GetById(a => a.Id == invoiceId, new string[] { "Supplier" });
            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة تسديد المورد : {invoice.Supplier.Name}.pdf"
            };
            return viewAsPdf;
        }


        // Report for  payment invoice for customer
        public IActionResult PayForCustomerReport(int invoiceId)
        {
            PaymentInvoiceCustomer invoice = _unitOfWork.PaymentInvoiceCustomerRepository
                .GetById(a => a.Id == invoiceId, new string[] { "Customer" });
            var viewAsPdf = new ViewAsPdf
            {
                Model = invoice,
                FileName = $"فاتورة تسديد العميل : {invoice.Customer.Name}.pdf"
            };
            return viewAsPdf;
        }

       
        // Report for End Shift
        public IActionResult EndShiftReport(EndShiftViewModel model)
        {
            model.Name = CurrentUser().FullName;
            model.Date = DateTime.Now;

            var viewAsPdf = new ViewAsPdf
            {
                Model = model,
                FileName = $"فاتورة انهاء شيفت : {model.Name}.pdf"
            };
            return viewAsPdf;
        }

    }
}
