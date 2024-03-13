using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;

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
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.CustomerRepository.AddAsync(customer);

                    return RedirectToAction("Index");
                }
                return View(customer);
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
            Customer customer = GetCustomer(id);
            if (customer != null)
            {
                try
                {
                    _unitOfWork.CustomerRepository.Delete(customer);
                    TempData["success"] = "تمت العملية بنجاح";

                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("Index");
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