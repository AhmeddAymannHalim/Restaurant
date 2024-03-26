using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class ScroungeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ScroungeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index()
        {
            var scrounges = _unitOfWork.ScroungeRepository
                .GetAll(a => a.Include(a => a.User), a => a.Include(a => a.Employee));

            return View(scrounges);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var scrounge = _unitOfWork.ScroungeRepository.GetById(a => a.Id == id, new string[] { "Employee", "User" });
            if (scrounge == null)
            {
                return NotFound();
            }
            return View(scrounge);
        }
       

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Scrounge model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");
                    model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    model.Date = DateTime.Now;

                    await _unitOfWork.ScroungeRepository.AddAsync(model);

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
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");

            var scrounge = _unitOfWork.ScroungeRepository.GetById(a => a.Id == id, new string[] { "Employee", "User" });
            if (scrounge == null)
            {
                return NotFound();
            }
            return View(scrounge);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Scrounge model)
        {
            try
            {
                ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");

                if (ModelState.IsValid)
                {
                    _unitOfWork.ScroungeRepository.Update(model);

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
            var obj = _unitOfWork.ScroungeRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.ScroungeRepository.Delete(obj);
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
