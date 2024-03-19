using ResturantPL.Helper;
using Restaurant.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResturantPL.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Service.UnitOfWork;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace ResturantPL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;

        }



        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }

        [Authorize(Roles = "Admin,Manager,كاشير")]
        public async Task<IActionResult> AdminDash()
        {
            ViewBag.CurrentAdmin = CurrentUser();
            AdminDashViewModel adminViewModel = new AdminDashViewModel()
            {
                Users = _userManager.Users.ToList(),
                Roles = _roleManager.Roles.ToList(),
                Branches = _unitOfWork.BrancheRepository.GetAll().ToList(),
                ChefItems = _unitOfWork.ChefItemRepository.GetAll().ToList(),
                Customers = _unitOfWork.CustomerRepository.GetAll().ToList(),
                Employees = _unitOfWork.EmployeeRepository.GetAll().ToList(),
                EmployeeAccounts = await _userManager.Users.Where(a => a.UserType != "User").ToListAsync(),
                Expenses = _unitOfWork.ExpenseRepository.GetAll().ToList(),
                Invoices = _unitOfWork.InvoiceRepository.GetAll().ToList(),
                InvoiceItems = _unitOfWork.InvoiceItemRepository.GetAll().ToList(),
                PaymentInvoiceSuppliers = _unitOfWork.PaymentInvoiceSupplierRepository.GetAll().ToList(),
                PaymentInvoiceCustomers = _unitOfWork.PaymentInvoiceCustomerRepository.GetAll().ToList(),
                Jobs = _unitOfWork.JobRepository.GetAll().ToList(),
                Products = _unitOfWork.ProductRepository.GetAll().ToList(),
                SalaryTypes = _unitOfWork.SalaryTypeRepository.GetAll().ToList(),
                Stores = _unitOfWork.StoreRepository.GetAll().ToList(),
                Suppliers = _unitOfWork.SupplierRepository.GetAll().ToList(),
                Tables = _unitOfWork.TableRepository.GetAll().ToList(),
                UnitPrice = _unitOfWork.UnitPriceRepository.GetAll().ToList(),
                IndexSettings = _unitOfWork.IndexRepository.GetAll().ToList(),
                MenuItems = _unitOfWork.MenuItemRepository.GetAll().ToList(),
                MenuSections = _unitOfWork.MenuSectionRepository.GetAll().ToList(),
                StoreConverts = _unitOfWork.StoreConvertRepository.GetAll().ToList(),
                Deposits = _unitOfWork.DepositRepository.GetAll().ToList(),
                kitchens = _unitOfWork.kitchenRepository.GetAll().ToList(),
                Scrounges = _unitOfWork.ScroungeRepository.GetAll().ToList(),
                ExitExpenses = _unitOfWork.ExitExpenseRepository.GetAll().ToList(),
                Salaries = _unitOfWork.SalaryRepository.GetAll().ToList(),
            };

            return View(adminViewModel);
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    ApplicationUser user = new ApplicationUser()
                    {
                        FirstName = registerViewModel.FirstName,
                        LastName = registerViewModel.LastName,
                        Email = registerViewModel.Email,
                        UserName = registerViewModel.Email.Split("@")[0],
                        PhoneNumber = registerViewModel.PhoneNumber,
                        UserType = "User",
                    };


                    var result = await _userManager.CreateAsync(user, registerViewModel.Password);
                    if (result.Succeeded)
                    {
                        var roleFound = await _roleManager.FindByNameAsync("User");
                        if (roleFound == null)
                        {
                            await _roleManager.CreateAsync(new IdentityRole { Name = "User" });

                        }
                        await _userManager.AddToRoleAsync(user, "User");
                        return RedirectToAction("SignIn");
                    }

                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(registerViewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AddUser()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");


            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            try
            {
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                if (ModelState.IsValid)
                {

                    ApplicationUser user = new ApplicationUser()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Email.Split("@")[0],
                        PhoneNumber = model.PhoneNumber,
                        UserType = model.UserType,
                    };


                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {

                        await _userManager.AddToRoleAsync(user, user.UserType);
                        return RedirectToAction("Index", "User");
                    }
                    else
                    {

                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        public IActionResult SignIn()
        {
            return View(new SignInViewModel());
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel signInViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(signInViewModel.Email);
                    if (user is not null)
                    {
                        var result = await _userManager.CheckPasswordAsync(user, signInViewModel.Password);
                        if (result)
                        {
                            
                            var roles = await _userManager.GetRolesAsync(user);

                           
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                                new Claim(ClaimTypes.Name, user.UserName), 
                                new Claim(ClaimTypes.Email, user.Email),
                            };

                           
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }

                          
                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);

                           
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                            return RedirectToAction("Index", "Home");
                        }
                        ModelState.AddModelError(string.Empty, "Password is not correct");
                    }
                    ModelState.AddModelError(string.Empty, "Email is not found");
                }
                return View(signInViewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn", "Account");
        }

        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordViewModel());
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user is not null)
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var ResetPasswordLink = Url.Action("ResetPassword", "Account", new { Email = model.Email, Token = token }, Request.Scheme);
                        var email = new Email()
                        {
                            Title = "Reset Password",
                            Body = ResetPasswordLink,
                            To = model.Email
                        };
                        EmailSettings.SendEmail(email);
                        return RedirectToAction("CheckYourInbox", "Account");
                    }
                    ModelState.AddModelError("", "Enter a valid email");
                }


                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult ResetPassword(string email, string token)
        {
            var resetPasswordViewModel = new ResetPasswordViewModel() { Email = email, Token = token };

            return View(resetPasswordViewModel);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user is not null)
                    {
                        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("SignIn");
                        }
                        foreach (var error in result.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest();

                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult CheckYourInbox()
        {
            return View();
        }


    }
}
