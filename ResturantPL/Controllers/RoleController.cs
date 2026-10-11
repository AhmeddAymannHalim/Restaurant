using AutoMapper;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using ResturantPL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;
using Restaurant.DAL.Models;

namespace ResturantPL.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {

        public IMapper _mapper { get; set; }
        public UserManager<ApplicationUser> _userManager { get; set; }
        public RoleManager<IdentityRole> _roleManager { get; set; }

        public RoleController(IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Roles()
        {
            IEnumerable<RoleViewModel> roleViewModels = null;

            roleViewModels = await _roleManager.Roles.Select(r => new RoleViewModel()
            {
                Id = r.Id,
                RoleName = r.Name

            }).ToListAsync();

            return View(roleViewModels);
        }


        [HttpGet]
        public IActionResult AddRole()
        {
            return View();

        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(RoleViewModel roleViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IdentityRole role = new IdentityRole()
                    {
                        Name = roleViewModel.RoleName
                    };
                    await _roleManager.CreateAsync(role);

                    return RedirectToAction("Roles");
                }
                return View(roleViewModel);

            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);


            if (role is null)
            {
                return BadRequest();
            }

            RoleViewModel roleViewModel = new RoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name
            };


            return View(roleViewModel);

        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RoleViewModel roleViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var role = await _roleManager.FindByIdAsync(roleViewModel.Id);

                    if (role.Name != roleViewModel.RoleName)
                        role.Name = roleViewModel.RoleName;


                    await _roleManager.UpdateAsync(role);

                    return RedirectToAction("Roles");
                }
                return View(roleViewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            return await Update(id);
        }

       

        //public async Task<IActionResult> Delete(string id)
        //{
        //    var obj = await _roleManager.FindByIdAsync(id);
        //    if (obj != null)
        //    {
        //        try
        //        {
        //            await _roleManager.DeleteAsync(obj);
        //            return Json(new { success = true });
        //        }
        //        catch (Exception)
        //        {
        //            return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
        //        }
        //    }
        //    return Json(new { success = true });
        //}

        [HttpGet]
        public async Task<IActionResult> Delete(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("Role ID مطلوب.");
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            RoleViewModel model = new RoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(RoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return BadRequest("Role ID مطلوب.");
            }

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return NotFound("الفئة غير موجودة.");
            }

            try
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles"); 
                }
                else
                {
                    return BadRequest("فشل حذف الفئة.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر");
            }
        }



        #region Old Roles

        //public async Task<IActionResult> Index(string SearchValue = "")
        //{


        //    IEnumerable<RoleViewModel> roleViewModels = null;
        //    if (SearchValue == null)
        //    {
        //        roleViewModels = await _roleManager.Roles.Select(r => new RoleViewModel()
        //        {
        //            Id = r.Id,
        //            RoleName = r.Name

        //        }).ToListAsync();


        //    }
        //    else
        //    {
        //        roleViewModels = await _roleManager.Roles.
        //            Where(r => r.Name.Trim().ToLower().Contains(SearchValue.Trim().ToLower()) == true).
        //            Select(r => new RoleViewModel()
        //            {
        //                Id = r.Id,
        //                RoleName = r.Name
        //            }).ToListAsync();

        //    }

        //    return View(roleViewModels);
        //}


        //[HttpGet]
        //public IActionResult AddRole()
        //{
        //    return View();

        //}
        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public async Task<IActionResult> AddRole(RoleViewModel roleViewModel)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        IdentityRole role = new IdentityRole()
        //        {
        //            Name = roleViewModel.RoleName
        //        };
        //        await _roleManager.CreateAsync(role);

        //        return RedirectToAction("Index");
        //    }
        //    return View(roleViewModel);

        //}


        //[HttpGet]
        //public async Task<IActionResult> Update([FromRoute]string id)
        //{
        //    var role = await _roleManager.FindByIdAsync(id);


        //    if (role is null)
        //    {
        //        return BadRequest();
        //    }

        //    RoleViewModel roleViewModel = new RoleViewModel()
        //    {
        //        Id = role.Id,
        //        RoleName = role.Name
        //    };


        //    return View(roleViewModel);

        //}
        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public async Task<IActionResult> Update(RoleViewModel roleViewModel)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        var role = await _roleManager.FindByIdAsync(roleViewModel.Id);

        //        if (role.Name != roleViewModel.RoleName)
        //            role.Name = roleViewModel.RoleName;


        //        await _roleManager.UpdateAsync(role);

        //        return RedirectToAction("Index");
        //    }
        //    return View(roleViewModel);

        //}

        //[HttpGet]
        //public async Task<IActionResult> Details([FromRoute] string id)
        //{


        //    return await Update(id);
        //}


        //[HttpGet]
        //public async Task<IActionResult> Delete([FromRoute] string id)
        //{

        //    return await Update(id);

        //}
        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public async Task<IActionResult> Delete(RoleViewModel roleViewModel)
        //{

        //    if (roleViewModel == null)
        //    {
        //        return BadRequest();
        //    }
        //    var role = await _roleManager.FindByIdAsync(roleViewModel.Id);
        //    await _roleManager.DeleteAsync(role);

        //    return RedirectToAction("Index");



        //}


        #endregion

    }
}
