using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ResturantPL.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _unitOfWork.CustomerRepository.GetAllAsync();

            return View(customers);
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            Customer customer = GetCustomer(id);

            if (customer != null)
            {
                return View(customer);
            }
            return NotFound();
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    string nameWithoutSpaces = model.Name.Replace(" ", "");

                    bool nameExist = _unitOfWork.CustomerRepository.Any(t => t.Name.Replace(" ", "") == nameWithoutSpaces);
                    bool mobileNumber = _unitOfWork.CustomerRepository.Any(t => t.MobileNumber == model.MobileNumber || t.PhoneNumber == model.MobileNumber);
                    bool phoneNumber = _unitOfWork.CustomerRepository.Any(t => t.PhoneNumber == model.PhoneNumber || t.MobileNumber == model.PhoneNumber);

                    if (nameExist)
                    {
                        ModelState.AddModelError("Name", "هذا العميل موجود بالفعل من فضلك قم بتغيير الاسم");
                        return View(model);
                    }

                    if (mobileNumber)
                    {
                        ModelState.AddModelError("MobileNumber", "هذا الرقم موجود بالفعل");
                        return View(model);
                    }

                    if (phoneNumber)
                    {
                        ModelState.AddModelError("PhoneNumber", "هذا الرقم موجود بالفعل");
                        return View(model);
                    }








                    await _unitOfWork.CustomerRepository.AddAsync(model);

                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = GetCustomer(id);
            if (customer != null)
            {
                return View(customer);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.CustomerRepository.Update(customer);

                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.CustomerRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.CustomerRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }







        public Customer GetCustomer(int id)
        {
            Customer customer = _unitOfWork.CustomerRepository.GetById(a => a.Id == id, new string[] { "PaymentInvoices" });
            return customer;
        }
        public IActionResult CustomerBalance(int customerId)
        {
            Customer customer = _unitOfWork.CustomerRepository.GetById(customerId);
            if (customer != null)
            {
                return Json(customer.Balance);
            }
            return BadRequest();
        }

    }
}