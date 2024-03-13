using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using System.Reflection.Metadata.Ecma335;

namespace ResturantPL.Controllers
{
    public class UnitPriceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public UnitPriceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var units = await _unitOfWork.UnitPriceRepository.GetAllAsync();
            return View(units);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitPrice unit)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.UnitPriceRepository.AddAsync(unit);
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(unit);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitOfWork.UnitPriceRepository.GetByIdAsync(id);
            if (unit != null)
            {
                return View(unit);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(UnitPrice unit)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.UnitPriceRepository.Update(unit);

                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(unit);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Delete(int id)
        {
            var unit = _unitOfWork.UnitPriceRepository.GetById(id);
            if (unit != null)
            {
                try
                {
                    _unitOfWork.UnitPriceRepository.Delete(unit);
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
    }
}
