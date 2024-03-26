using AutoMapper;
using ResturantPL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResturantPL.Helper;
using Restaurant.Data.Models;

namespace ResturantPL.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }




        public async Task<IActionResult> Index(string SearchValue = "")
        {
            IEnumerable<UsersViewModel> usersViewModel = null;
            var users = await _userManager.Users.ToListAsync();

            if (string.IsNullOrWhiteSpace(SearchValue))
            {
                var groupedUsers = users
                    .OrderBy(u => u.UserType)
                    .GroupBy(u => u.UserType)
                    .SelectMany(group =>
                        group.Concat(new[] { new ApplicationUser() }) // Add empty row after each group
                    )
                    .Select(u => new UsersViewModel()
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Name = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        UserType = u.UserType,
                    });

                usersViewModel = groupedUsers.ToList();
            }
            else
            {
                usersViewModel = users
                    .Where(u => u.Email.Trim().ToLower().Contains(SearchValue.Trim().ToLower()) || u.FullName.ToLower().Contains(SearchValue.ToLower()) || u.UserType.ToLower().Contains(SearchValue.ToLower()) || u.UserName.ToLower().Contains(SearchValue.ToLower()))
                    .Select(u => new UsersViewModel()
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Name = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        UserType = u.UserType,
                    })
                    .OrderBy(a => a.UserType)
                    .ToList();
            }

            return View(usersViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Update(string id, string ViewName = "Update")
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return BadRequest();
            }

            UsersViewModel usersViewModel = new UsersViewModel()
            {
                Id = user.Id,
                Name = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
            };

            return View(ViewName, usersViewModel);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UsersViewModel usersViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(usersViewModel.Id);
                    if (user.PhoneNumber != usersViewModel.PhoneNumber)
                        user.PhoneNumber = usersViewModel.PhoneNumber;
                    if (user.UserName != usersViewModel.UserName)
                        user.UserName = usersViewModel.UserName;
                    if (user.Email != usersViewModel.Email)
                        user.Email = usersViewModel.Email;
                    if (user.FirstName != usersViewModel.Name.Split(" ")[0])
                        user.FirstName = usersViewModel.Name.Split(" ")[0];
                    if (user.LastName != usersViewModel.Name.Split(" ")[1])
                        user.LastName = usersViewModel.Name.Split(" ")[1];

                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("Index");
                }
                return View(usersViewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }

        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            return await Update(id, "Details");
        }


        
        [HttpGet]
        public async Task<IActionResult> Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID مطلوب.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            UsersViewModel model = new UsersViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.FullName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                UserType = user.UserType,
            };

            return View(model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(UsersViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return BadRequest("User ID مطلوب.");
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index"); // Redirect to the index page
                }
                else
                {
                    return BadRequest("فشل حذف المستخدم.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر");
            }
        }



        //public async Task<IActionResult> Delete(string userId)
        //{
        //    var obj = await _userManager.FindByIdAsync(userId);
        //    if (obj != null)
        //    {
        //        try
        //        {
        //            await _userManager.DeleteAsync(obj);
        //            return Json(new { success = true });
        //        }
        //        catch (Exception)
        //        {
        //            return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
        //        }
        //    }
        //    return Json(new { success = true });
        //}

    }
}
