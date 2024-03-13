using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ResturantPL.Controllers
{
    public class ExitExpenseController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public ExitExpenseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var expenses = _unitOfWork.ExitExpenseRepository.
                GetAll(a => a.Include(a => a.Expense), a => a.Include(a => a.User));
            return View(expenses);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Expenses = new SelectList(_unitOfWork.ExpenseRepository.GetAll(), "Id", "Name");
            return View();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExitExpense expens)
        {
            try
            {
                ViewBag.Expenses = new SelectList(_unitOfWork.ExpenseRepository.GetAll(), "Id", "Name");
                if (ModelState.IsValid)
                {
                    expens.Date = DateTime.Now;
                    expens.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


                    await _unitOfWork.ExitExpenseRepository.AddAsync(expens);

                    return RedirectToAction("Index");
                }
                return View(expens);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _unitOfWork.ExitExpenseRepository.GetByIdAsync(id);
            if (expense != null)
            {
                return View(expense);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(ExitExpense expens)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.ExitExpenseRepository.Update(expens);

                    _unitOfWork.Complete();
                    return RedirectToAction("Index");
                }
                return View(expens);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult Delete(int id)
        {
            var exitExpense = _unitOfWork.ExitExpenseRepository.GetById(id);
            if (exitExpense != null)
            {
                try
                {
                    _unitOfWork.ExitExpenseRepository.Delete(exitExpense);
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