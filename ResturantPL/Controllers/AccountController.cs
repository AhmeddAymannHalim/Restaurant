using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Services;
using Shared;
using Shared.ViewModels.Definitions;
using System.Security.Claims;
using System.Text;

namespace ResturantPL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _service;
        private readonly IEmailService _email;
        private readonly ILookupService _lookup;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountController(IAccountService service, IEmailService email, ILookupService lookup, IStringLocalizer<SharedResource> localizer)
        {
            _service = service;
            _email = email;
            _lookup = lookup;
            _localizer = localizer;
        }

        [Authorize]
        public IActionResult AdminDash()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.RegisterAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, _localizer[result.Message].Value);
                return View(model);
            }

            return RedirectToAction(nameof(SignIn));
        }

        public IActionResult SignIn(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new SignInViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.SignInAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, _localizer[result.Message].Value);
                ViewBag.ShowResend = result.Message == "Auth_ConfirmFirst";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Data.Id),
                new(ClaimTypes.Name, result.Data.UserName),
                new(ClaimTypes.Email, result.Data.Email),
            };
            claims.AddRange(result.Data.Roles
                  .Select(role => new Claim(ClaimTypes.Role, role)));

            if (!string.IsNullOrEmpty(result.Data.Image))
            {
                claims.Add(new Claim("Image", result.Data.Image));
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Dashboard");
        }

        public new async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(SignIn));
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.ChangePasswordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, _localizer[result.Message].Value);
                return View(model);
            }

            TempData["PasswordChanged"] = true;
            return RedirectToAction(nameof(ChangePassword));
        }

        public IActionResult CheckYourInbox(string type = "confirm")
        {
            ViewBag.Type = type;
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token ?? string.Empty));
            var result = await _service.ConfirmEmailAsync(userId, decodedToken);

            return View(result.Success);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            var token = await _service.GetConfirmationTokenAsync(email);
            if (token is not null)
            {
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token.Token));
                var confirmLink = Url.Action(nameof(ConfirmEmail), "Account", new
                {
                    userId = token.UserId,
                    token = encodedToken
                }, Request.Scheme);
                var confirmText = $"Hi {token.FirstName}, welcome! Please confirm your email address to activate your account.";

                await _email.SendAsync(token.Email, "Confirm your email",
                    $@"<p style=""color:#c9cfe4"">{confirmText}</p>
<p><a href=""{confirmLink}"" style=""display:inline-block;background:#b85c68;color:#fff;text-decoration:none;padding:12px 28px;border-radius:999px;font-weight:600"">Confirm Email</a></p>
<p style=""color:#8b93ad;font-size:12px"">If you didn't request this, you can ignore this email.</p>");
            }

            return RedirectToAction(nameof(CheckYourInbox), new { type = "confirm" });
        }

        public IActionResult ForgetPassword()
        {
            return View(new ForgetPasswordViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = await _service.GetPasswordResetTokenAsync(model.Email);
            if (token is not null)
            {
                var link = Url.Action(nameof(ResetPassword), "Account", new
                {
                    email = token.Email,
                    token = token.Token
                }, Request.Scheme);
                await _email.SendAsync(token.Email,
                    "Reset your password",
                    $@"<p style=""color:#c9cfe4"">Click the button below to choose a new password.</p>
<p><a href=""{link}"" style=""display:inline-block;background:#b85c68;color:#fff;text-decoration:none;padding:12px 28px;border-radius:999px;font-weight:600"">Reset Password</a></p>
<p style=""color:#8b93ad;font-size:12px"">If you didn't request this, you can ignore this email.</p>");
            }

            return RedirectToAction(nameof(CheckYourInbox), new { type = "reset" });
        }

        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.ResetPasswordAsync(model);
            if (!result.Success && result.Message != "RecordNotFound")
            {
                ModelState.AddModelError(string.Empty, _localizer[result.Message].Value);
                return View(model);
            }

            return RedirectToAction(nameof(SignIn));
        }
    }
}
