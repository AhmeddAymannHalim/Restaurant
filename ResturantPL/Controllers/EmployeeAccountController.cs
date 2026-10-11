using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using ResturantPL.ViewModels;
using Shared.ViewModels.Commons;

namespace ResturantPL.Controllers
{
    // Login accounts of the employees (ASP.NET Identity). The employee definition itself lives in Controllers/Definitions.
    [Authorize]
    public class EmployeeAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeAccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Employee GetEmployee(int id)
        {
            Employee employee = _unitOfWork.EmployeeRepository
                .GetById(a => a.EmployeeId == id, new string[] { "SalaryType", "Branche", "Job" });

            return employee;
        }

        public IActionResult CheckAccount(int id)
        {
            Employee employee = GetEmployee(id);
            if (employee != null)
            {
                ApplicationUser user = _userManager.Users.Where(a => a.Code == employee.Code).FirstOrDefault();
                if (user != null)
                {
                    return RedirectToAction("UpdateAccount", user);

                }
                return RedirectToAction("AddAccount", employee);
            }
            return NotFound();
        }

        public async Task<IActionResult> UpdateUserAccount(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return RedirectToAction("UpdateAccount", user);
            }
            return NotFound();
        }


        public IActionResult NewAccount()
        {
            var userCodes = _userManager.Users.Select(u => u.Code).ToList();
            var employees = new List<Employee>();
            foreach (var code in userCodes)
            {
                 employees = _unitOfWork.EmployeeRepository.GetAll()
                               .Where(e => e.Code != code).ToList();
            }

            ViewBag.Employees = new SelectList(employees, "EmployeeId", "FullName");
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAccount(NewAccountForEmployeeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Employee employee = GetEmployee(model.EmployeeId);
                    ApplicationUser user = new ApplicationUser()
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Country = employee.Country,
                        City = employee.City,
                        Street = employee.Street,
                        Age = employee.Age,
                        Image = employee.Image,
                        PhoneNumber = employee.PhoneNumber,
                        MobileNumber = employee.MobileNumber,
                        UserType = employee.UserType,
                        Code = employee.Code,
                        Email = model.Email,
                        UserName = model.UserName
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var found = await _roleManager.FindByNameAsync(user.UserType);
                        if (found == null)
                        {
                            var addRole = await _roleManager.CreateAsync(new IdentityRole(user.UserType));
                            if (addRole.Succeeded)
                            {
                                var role = await _userManager.AddToRoleAsync(user, user.UserType);
                                if (role.Succeeded)
                                {
                                    return RedirectToAction("EmployeeAccounts");
                                }
                                else
                                {
                                    foreach (var error in role.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var error in addRole.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }
                        else
                        {
                            var role = await _userManager.AddToRoleAsync(user, user.UserType);
                            if (role.Succeeded)
                            {
                                return RedirectToAction("EmployeeAccounts");
                            }
                            else
                            {
                                foreach (var error in role.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }

                    }
                    foreach (var res in result.Errors)
                    {
                        ModelState.AddModelError("", res.Description);
                    }

                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }


        public IActionResult AddAccount(Employee model)
        {
            AddAccountForEmployee viewModel = new AddAccountForEmployee()
            {
                EmployeeId = model.EmployeeId,
            };

            return View(viewModel);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccount(AddAccountForEmployee model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Employee employee = GetEmployee(model.EmployeeId);
                    ApplicationUser user = new ApplicationUser()
                    {
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Country = employee.Country,
                        City = employee.City,
                        Street = employee.Street,
                        Age = employee.Age,
                        Image = employee.Image,
                        PhoneNumber = employee.PhoneNumber,
                        MobileNumber = employee.MobileNumber,
                        UserType = employee.UserType,
                        Code = employee.Code,
                        Email = model.Email,
                        UserName = model.UserName
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var found = await _roleManager.FindByNameAsync(user.UserType);
                        if (found == null)
                        {
                            var addRole = await _roleManager.CreateAsync(new IdentityRole(user.UserType));
                            if (addRole.Succeeded)
                            {
                                var role = await _userManager.AddToRoleAsync(user, user.UserType);
                                if (role.Succeeded)
                                {
                                    return RedirectToAction("EmployeeAccounts");
                                }
                                else
                                {
                                    foreach (var error in role.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var error in addRole.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }
                        else
                        {
                            var role = await _userManager.AddToRoleAsync(user, user.UserType);
                            if (role.Succeeded)
                            {
                                return RedirectToAction("EmployeeAccounts");
                            }
                            else
                            {
                                foreach (var error in role.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }

                    }
                    foreach (var res in result.Errors)
                    {
                        ModelState.AddModelError("", res.Description);
                    }

                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }


        public IActionResult UpdateAccount(ApplicationUser model)
        {
            AddAccountForEmployee viewModel = new AddAccountForEmployee()
            {
                Email = model.Email,
                UserName = model.UserName,
                Id = model.Id
            };


            return View(viewModel);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AddAccountForEmployee model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser user = await _userManager.FindByIdAsync(model.Id);

                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("EmployeeAccounts");
                    }
                    foreach (var res in result.Errors)
                    {
                        ModelState.AddModelError("", res.Description);
                    }

                }

                return View(model);
            }
            catch (Exception ex)
            {
                return ErrorHandler.ErrorView(ex.Message);
            }
        }


        public async Task<IActionResult> EmployeeAccounts()
        {
            List<ApplicationUser> models = await _userManager.Users
                .Where(a => a.UserType != "User").ToListAsync();

            return View(models);
        }
    }
}
