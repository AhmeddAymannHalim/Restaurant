using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class SalaryController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public SalaryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public ActionResult Index()
        {
            var salary = _unitOfWork.SalaryRepository
                .GetAll(a => a.Include(a => a.User), a => a.Include(a => a.Employee));
            return View(salary);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var salary = _unitOfWork.SalaryRepository.GetById(a => a.Id == id, new string[] { "Employee", "User" });
            if (salary == null)
            {
                return NotFound();
            }
            return View(salary);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");

            Salary salary = await _unitOfWork.SalaryRepository.GetLast(item => item.Id);

            if (salary == null)
            {
                ViewBag.SalaryCode = 1;
            }
            else
            {
                int salaryCode = int.Parse(salary.SalaryCode) + 1;
                ViewBag.SalaryCode = salaryCode;
            }
            ViewBag.DateNow = DateTime.Now.ToString("yyyy-MM-dd");

            return View();


        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Salary model)
        {
            try
            {
                ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");
                if (ModelState.IsValid)
                {
                    model.SalaryDate = DateTime.Now;
                    model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


                    await _unitOfWork.SalaryRepository.AddAsync(model);

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
            var salary = _unitOfWork.SalaryRepository.GetById(a => a.Id == id, new string[] { "Employee", "User" });
            if (salary == null)
            {
                return NotFound();
            }
            return View(salary);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Salary model)
        {
            try
            {
                ViewBag.Employees = new SelectList(_unitOfWork.EmployeeRepository.GetAll(), "EmployeeId", "FullName");
                if (ModelState.IsValid)
                {
                    _unitOfWork.SalaryRepository.Update(model);

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
            var salary = _unitOfWork.SalaryRepository.GetById(id);
            if (salary != null)
            {
                try
                {
                    _unitOfWork.SalaryRepository.Delete(salary);
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



        public IActionResult EmployeeExpenses(int employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = _unitOfWork.EmployeeRepository.GetById(a => a.EmployeeId == employeeId, new string[] { "SalaryType"});

            EmployeeExpense expenses = new EmployeeExpense()
            {
                Incentives = _unitOfWork.ScroungeRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.EmployeeId == employeeId && a.State == MoneyState.Incentive).Sum(a => a.Amount),
                Depts = _unitOfWork.ScroungeRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.EmployeeId == employeeId && a.State == MoneyState.Dept).Sum(a => a.Amount),
                Discounts = _unitOfWork.ScroungeRepository.GetAll()
                .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date && a.EmployeeId == employeeId && a.State == MoneyState.Discount).Sum(a => a.Amount),
                MainSalary = employee.Salary,
                SalaryType = employee.SalaryType.Type
            };
            expenses.TotalSalary = expenses.MainSalary + expenses.Incentives - expenses.Discounts - expenses.Depts;
            return Json(expenses);
        }

    }








}

