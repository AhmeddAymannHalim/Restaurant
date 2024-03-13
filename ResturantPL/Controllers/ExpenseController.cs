using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using System.Reflection.Metadata.Ecma335;

namespace ResturantPL.Controllers
{
    public class ExpenseController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public ExpenseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var expenses = await _unitOfWork.ExpenseRepository.GetAllAsync();
            return View(expenses);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.ExpenseRepository.AddAsync(expense);

                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(expense);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _unitOfWork.ExpenseRepository.GetByIdAsync(id);
            if (expense != null)
            {
                return View(expense);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Expense expense)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.ExpenseRepository.Update(expense);

                    _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                return View(expense);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Delete(int id)
        {
            var expense = _unitOfWork.ExpenseRepository.GetById(id);
            if (expense != null)
            {
                try
                {
                    _unitOfWork.ExpenseRepository.Delete(expense);
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
