using Microsoft.AspNetCore.Identity;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.UnitOfWork;
using ResturantPL.Helper;
using System.Security.Claims;
using Restaurant.DAL.Models;

namespace ResturantPL.Controllers
{


    public class ShiftController : Controller
    {

        //string pass = "@Qwe12";
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShiftController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
        }


        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }


        public IActionResult Login()
        {
            try
            {
                ViewBag.CurrentUser = CurrentUser(); ;

                return View();
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string password)
        {
            try
            {
                ViewBag.CurrentUser = CurrentUser();


                ApplicationSetting setting = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);

                if (setting.Password == password)
                {
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    return RedirectToAction("Create", "Order");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public async Task<IActionResult> Logout(string password)
        {
            try
            {
                ViewBag.CurrentUser = CurrentUser();

                ApplicationSetting setting = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);

                if (setting.Password == password)
                //if (PasswordManager.ValidatePassword(password))
                {
                    var orders = _unitOfWork.OrderRepository.GetAll().Where(a => a.Cash && a.ReceiverId == CurrentUser().Id);
                    foreach (var item in orders)
                    {
                        item.FinishShift = true;
                        await _unitOfWork.OrderRepository.UpdateAsync(item);
                    }

                    HttpContext.Session.SetString("IsAuthenticated", "false");
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return RedirectToAction("Index", "Order");
                }

            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



    }
}
