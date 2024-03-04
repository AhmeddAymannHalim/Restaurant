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


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _unitOfWork.UnitPriceRepository.GetByIdAsync(id);
            if (unit != null)
            {
                return View(unit);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteUnitType(int id)
        {
            try
            {
                var unit = await _unitOfWork.UnitPriceRepository.GetByIdAsync(id);
                if (unit != null)
                {
                    _unitOfWork.UnitPriceRepository.Delete(unit);
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }
    }
}
