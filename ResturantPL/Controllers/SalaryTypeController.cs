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


        public async Task<IActionResult> DeleteSalaryType(int id)
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


        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteSalaryType")]
        public async Task<IActionResult> DeleteSalaryTypeConfirm(int id)
        {
            try
            {
                SalaryType type = await GetSalary(id);
                if (type != null)
                {
                    _unitOfWork.SalaryTypeRepository.Delete(type);

                    return RedirectToAction("Index");
                }
                return BadRequest();

            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public async Task<SalaryType> GetSalary(int id)
        {
            SalaryType type = await _unitOfWork.SalaryTypeRepository.GetByIdAsync(id);
            return type;
        }

    }
}
