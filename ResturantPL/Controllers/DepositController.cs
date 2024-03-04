using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using ResturantPL.ViewModels;
using Service.UnitOfWork;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Deposit deposit)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                deposit.CreatedTime = DateTime.Now;
                deposit.UserId = userId;
                deposit.IsDeleted = false;

                await _unitOfWork.DepositRepository.AddAsync(deposit);

                return RedirectToAction("Index");
            }
            return View(deposit);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if ( id == 0)
            {
               return NotFound();
            }
            var depositIndb = await _unitOfWork.DepositRepository.GetByIdAsync(id);

            return View(depositIndb);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Deposit deposit)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.DepositRepository.Update(deposit);

                return RedirectToAction("Index");
            }
            return View(deposit);
        }

      

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
              return  NotFound();
            }

            var depositIndb = _unitOfWork.DepositRepository.GetById(a => a.Id == id, new string[] { "User" });

            return View(depositIndb);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public IActionResult DeleteDeposit(int id)
        {
            var depositIndb = _unitOfWork.DepositRepository.GetFirstorDefault(x => x.Id == id);

            if (depositIndb == null)
            {
                return NotFound();
            }
            depositIndb.IsDeleted = true;

            _unitOfWork.DepositRepository.Update(depositIndb);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            if ( id == 0)
            {
               return NotFound();
            }

            var depositIndb = _unitOfWork.DepositRepository.GetById(a => a.Id == id, new string[]{ "User"});

            return View(depositIndb);
        }
    }
}
