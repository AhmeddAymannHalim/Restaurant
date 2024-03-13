using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.Helper;
using ResturantPL.ViewModels;
using StackExchange.Redis;
using System.IO;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }



        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }



        public IActionResult Employees()
        {
            IEnumerable<Employee> models = _unitOfWork.EmployeeRepository.GetAll(
                a => a.Include(x => x.Branche),
                a => a.Include(a => a.SalaryType),
                a => a.Include(a => a.Job));


            var users = _userManager.Users;
            ViewBag.Users = users;

            return View(models);
        }



        public IActionResult DetailsEmployee(int id)
        {
            Employee employee = GetEmployee(id);
            if (employee != null)
            {
                return View(employee);
            }
            return NotFound();
        }



        public IActionResult AddEmployee()
        {
            var branches = _unitOfWork.BrancheRepository.GetAll();
            var jobs = _unitOfWork.JobRepository.GetAll();
            var salaryTypes = _unitOfWork.SalaryTypeRepository.GetAll();

            ViewBag.BranchList = new SelectList(branches, "Id", "Name");
            ViewBag.JobList = new SelectList(jobs, "JobId", "Name");
            ViewBag.SalaryType = new SelectList(salaryTypes, "Id", "Type");

            return View();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(EmployeeViewModel model)
        {
            try
            {
                ViewBag.BranchList = new SelectList(_unitOfWork.BrancheRepository.GetAll(), "Id", "Name");
                ViewBag.JobList = new SelectList(_unitOfWork.JobRepository.GetAll(), "JobId", "Name");
                ViewBag.SalaryType = new SelectList(_unitOfWork.SalaryTypeRepository.GetAll(), "Id", "Type");


                if (ModelState.IsValid)
                {
                    Job job = _unitOfWork.JobRepository.GetById(model.JobId);

                    Employee employee = new Employee()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Country = model.Country,
                        City = model.City,
                        Street = model.Street,
                        Age = model.Age,
                        PhoneNumber = model.PhoneNumber,
                        MobileNumber = model.MobileNumber,
                        UserType = job.Name,
                        Code = model.Code,
                        NationalID = model.NationalID,
                        SalaryTypeId = model.SalaryTypeId,
                        Salary = model.Salary,
                        BrancheId = model.BrancheId,
                        JobId = model.JobId
                    };

                    if (model.ImageUrl != null)
                    {
                        employee.Image = DocumentSetting.UploadFile(model.ImageUrl, "Employee Images");
                    }
                    await _unitOfWork.EmployeeRepository.AddAsync(employee);
                    return RedirectToAction("Employees");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult UpdateEmployee(int id)
        {
            ViewBag.BranchList = new SelectList(_unitOfWork.BrancheRepository.GetAll(), "Id", "Name");
            ViewBag.JobList = new SelectList(_unitOfWork.JobRepository.GetAll(), "JobId", "Name");
            ViewBag.SalaryType = new SelectList(_unitOfWork.SalaryTypeRepository.GetAll(), "Id", "Type");

            Employee employee = GetEmployee(id);
            if (employee != null)
            {
                EmployeeViewModel profile = new EmployeeViewModel()
                {
                    EmployeeId = employee.EmployeeId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    PhoneNumber = employee.PhoneNumber,
                    MobileNumber = employee.MobileNumber,
                    Country = employee.Country,
                    City = employee.City,
                    Street = employee.Street,
                    Age = employee.Age,
                    UserType = employee.UserType,
                    Code = employee.Code,
                    NationalID = employee.NationalID,
                    SalaryTypeId = employee.SalaryTypeId,
                    Salary = employee.Salary,
                    BrancheId = employee.BrancheId,
                    JobId = employee.JobId,
                };
                return View(profile);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(EmployeeViewModel model)
        {
            try
            {
                ViewBag.BranchList = new SelectList(_unitOfWork.BrancheRepository.GetAll(), "Id", "Name");
                ViewBag.JobList = new SelectList(_unitOfWork.JobRepository.GetAll(), "JobId", "Name");
                ViewBag.SalaryType = new SelectList(_unitOfWork.SalaryTypeRepository.GetAll(), "Id", "Type");

                if (ModelState.IsValid)
                {
                    Employee employee = GetEmployee(model.EmployeeId);

                    Job job = _unitOfWork.JobRepository.GetById(model.JobId);
                    string lastCode = null;
                    if (employee != null)
                    {
                        lastCode = employee.Code;

                        employee.FirstName = model.FirstName;
                        employee.LastName = model.LastName;
                        employee.Country = model.Country;
                        employee.City = model.City;
                        employee.Street = model.Street;
                        employee.Age = model.Age;
                        employee.PhoneNumber = model.PhoneNumber;
                        employee.MobileNumber = model.MobileNumber;
                        employee.UserType = job.Name;
                        employee.Code = model.Code;
                        employee.NationalID = model.NationalID;
                        employee.SalaryTypeId = model.SalaryTypeId;
                        employee.Salary = model.Salary;
                        employee.BrancheId = model.BrancheId;
                        employee.JobId = model.JobId;

                    }

                    if (model.ImageUrl != null)
                    {
                        if (employee.Image != null)
                        {
                            DocumentSetting.DeleteFile(employee.Image);
                        }
                        employee.Image = DocumentSetting.UploadFile(model.ImageUrl, "Employee Images");
                    }

                    await _unitOfWork.EmployeeRepository.UpdateAsync(employee);

                    ApplicationUser user = _userManager.Users.Where(a => a.Code == lastCode).SingleOrDefault();

                    if (user != null)
                    {
                        string lastRole = user.UserType;

                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.Country = model.Country;
                        user.City = model.City;
                        user.Street = model.Street;
                        user.Age = model.Age;
                        user.PhoneNumber = model.PhoneNumber;
                        user.MobileNumber = model.MobileNumber;
                        user.UserType = job.Name;
                        user.Code = model.Code;

                        await _userManager.UpdateAsync(user);

                        await _userManager.RemoveFromRolesAsync(user, new[] { lastRole.ToString() });

                        var found = await _roleManager.FindByNameAsync(job.Name);
                        if (found == null)
                        {
                            var addRole = await _roleManager.CreateAsync(new IdentityRole(job.Name));
                            if (addRole.Succeeded)
                            {
                                var role = await _userManager.AddToRoleAsync(user, job.Name);
                                if (role.Succeeded)
                                {
                                    return RedirectToAction("Employees");
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
                            var role = await _userManager.AddToRoleAsync(user, job.Name);
                            if (role.Succeeded)
                            {
                                return RedirectToAction("Employees");
                            }
                            else
                            {
                                foreach (var error in role.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }



                        await _userManager.AddToRoleAsync(user, job.Name);

                        return RedirectToAction("Employees");
                    }


                    return RedirectToAction("Employees");


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
            var employee = _unitOfWork.EmployeeRepository.GetById(id);
            if (employee != null)
            {
                try
                {
                    _unitOfWork.EmployeeRepository.Delete(employee);
                    TempData["success"] = "تمت العملية بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("Employees");
        }

        public Employee GetEmployee(int id)
        {
            Employee employee = _unitOfWork.EmployeeRepository
                .GetById(a => a.EmployeeId == id, new string[] { "SalaryType", "Branche", "Job" });
            if (employee != null)
            {
                return employee;
            }
            return null;
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
                return View("Error", ErrorMessage.Error(ex.Message));
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
                return View("Error", ErrorMessage.Error(ex.Message));
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
