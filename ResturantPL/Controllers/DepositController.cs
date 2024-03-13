using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using ResturantPL.ViewModels;
using Service.UnitOfWork;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class DepositController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IUnitOfWork _unitOfWork;

        public DepositController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager; 
        }

        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }


        public ActionResult Index()
        {
            var deposits = _unitOfWork.DepositRepository.GetAll(a => a.Include(a => a.User)).Where(a => a.IsDeleted == false);
            return View(deposits);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var lastDeposit = await _unitOfWork.DepositRepository.GetLast(item => item.Id);

            if (lastDeposit == null)
            {
                ViewBag.ProcessCode = 1;
            }
            else
            {
                int processCode = int.Parse(lastDeposit.ProcessCode) + 1;
                ViewBag.ProcessCode = processCode;
            }
            return View();
        }

  
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Deposit deposit)
        {
            if (ModelState.IsValid)
            {
                deposit.CreatedTime = DateTime.Now;
                deposit.UserId = CurrentUser().Id;
                deposit.IsDeleted = false;

                await _unitOfWork.DepositRepository.AddAsync(deposit);

                return RedirectToAction("Index");
            }
            return View(deposit);
        }


        public IActionResult Edit(int id)
        {
            var deposit = GetDeposit(id);
            if (deposit == null)
            {
               return NotFound();
            }

            return View(deposit);

        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Deposit deposit)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.DepositRepository.Update(deposit);

                return RedirectToAction("Index");
            }
            return View(deposit);
        }


        public IActionResult Delete(int id)
        {
            var deposit = GetDeposit(id);
            if (deposit != null)
            {
                try
                {
                    deposit.IsDeleted = true;
                    _unitOfWork.DepositRepository.Update(deposit);

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


        [HttpGet]
        public IActionResult Details(int id)
        {
            var deposit = GetDeposit(id);
            if (deposit == null)
            {
               return NotFound();
            }
            return View(deposit);
        }



        public Deposit GetDeposit(int id)
        {
            Deposit deposit = _unitOfWork.DepositRepository.GetById(a => a.Id == id, new string[] { "User" });
            return deposit;
        }
    }
}
