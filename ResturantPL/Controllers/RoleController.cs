using AutoMapper;
using ResturantPL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;
using Restaurant.Data.Models;

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


  
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                try
                {
                    await _roleManager.DeleteAsync(role);
                    TempData["success"] = "تمت العملية بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("Roles");
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
