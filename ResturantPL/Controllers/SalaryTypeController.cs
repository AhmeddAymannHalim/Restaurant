using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class SalaryTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SalaryTypeController(IUnitOfWork unitOfWork, IMapper _mapper)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.SalaryTypeRepository.GetAll());
        }
        public async Task<IActionResult> Details(int id)
        {
            SalaryType type = await GetSalary(id);
            if (type != null)
            {

                return View(type);
            }
            return NotFound();

        }

        public IActionResult AddSalaryType()
        {
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSalaryType(SalaryType model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.SalaryTypeRepository.AddAsync(model);
                    return RedirectToAction("Index");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public async Task<IActionResult> UpdateSalaryType(int id)
        {
            SalaryType type = await GetSalary(id);
            if (type != null)
            {
                return View(type);
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateSalaryType(SalaryType model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.SalaryTypeRepository.Update(model);
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
            var obj = _unitOfWork.SalaryTypeRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.SalaryTypeRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }
        public async Task<SalaryType> GetSalary(int id)
        {
            SalaryType type = await _unitOfWork.SalaryTypeRepository.GetByIdAsync(id);
            return type;
        }

    }
}
