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
    [Authorize]
    public class UserController : Controller
    {

        public IMapper _Mapper { get; set; }
        public UserManager<ApplicationUser> _userManager { get; set; }
        public RoleManager<IdentityRole> _roleManager { get; set; }

        public UserController(IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _Mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        public async Task<IActionResult> Index(string SearchValue = "")
        {
            IEnumerable<UsersViewModel> usersViewModel = null;
            if (SearchValue == null)
            {
                usersViewModel = await _userManager.Users.Where(a => a.UserType == "User").Select(u => new UsersViewModel()
                {
                    Id = u.Id,
                    Name = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Roles = _userManager.GetRolesAsync(u).Result


                }).ToListAsync();


            }
            else
            {
                usersViewModel = await _userManager.Users.
                    Where(u => u.Email.Trim().ToLower().Contains(SearchValue.Trim().ToLower()) == true && u.UserType == "User").
                    Select(u => new UsersViewModel()
                    {
                        Id = u.Id,
                        Name = u.UserName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                    }).ToListAsync();

            }

            return View(usersViewModel);
        }



        [HttpGet]
        public async Task<IActionResult> Update([FromRoute] string id, string ViewName = "Update")
        {
            var user = await _userManager.FindByIdAsync(id);


            if (user is null)
            {
                return BadRequest();
            }

            UsersViewModel usersViewModel = new UsersViewModel()
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
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
                    if (user.UserName != usersViewModel.Name)
                        user.UserName = usersViewModel.Name;
                    if (user.Email != usersViewModel.Email)
                        user.Email = usersViewModel.Email;

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
        public async Task<IActionResult> Details([FromRoute] string id)
        {
            return await Update(id, "Details");
        }



        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {

            return await Update(id, "Delete");

        }
     
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UsersViewModel usersViewModel)
        {
            try
            {
                if (usersViewModel == null)
                {
                    return BadRequest();
                }
                var user = await _userManager.FindByIdAsync(usersViewModel.Id);
                await _userManager.DeleteAsync(user);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

    }
}
