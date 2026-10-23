using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.BLL.Services.IServices.Definitions;
using ResturantPL.Controllers;
using ResturantPL.Services;
using Shared;
using Shared.Helpers.Localization;
using Shared.ViewModels.Commons;
using Shared.ViewModels.Definitions;
using System.Text;

namespace Restaurant.Tests;

// Sign-in, registration, e-mail confirmation and password screens, with the services, the cookie sign-in and the e-mail sender faked:
// what the user sees for every answer, and that a link is only mailed for an account that exists.
public class AccountControllerBranchTests
{
    private class Screen
    {
        public Mock<IAccountService> Service = new(MockBehavior.Loose);
        public Mock<IEmailService> Email = new();
        public Mock<IAuthenticationService> Auth = new();
        public AccountController Controller;
        public DefaultHttpContext Http = new();

        public Screen()
        {
            var localizer = new Mock<IStringLocalizer<SharedResource>>();
            localizer.Setup(a => a[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
            Auth.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);
            Auth.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).Returns(Task.CompletedTask);
            Http.RequestServices = new ServiceCollection().AddSingleton(Auth.Object).BuildServiceProvider();
            Http.Request.Scheme = "https";
            Http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }, "test"));

            var url = new Mock<IUrlHelper>();
            url.Setup(a => a.IsLocalUrl(It.IsAny<string>())).Returns((string value) => value is not null && value.StartsWith("/"));
            url.Setup(a => a.Action(It.IsAny<UrlActionContext>())).Returns("https://app/link");

            Controller = new AccountController(Service.Object, Email.Object, new Mock<ILookupService>().Object, localizer.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = Http },
                Url = url.Object,
                TempData = new TempDataDictionary(Http, Mock.Of<ITempDataProvider>())
            };
        }
    }

    // ---------------------------------------------------------------- register

    [Fact]
    public async Task Register_ShowsTheForm_InvalidOrRefusedShowsItAgain_SuccessGoesToSignIn()
    {
        var screen = new Screen();
        screen.Service.SetupSequence(a => a.RegisterAsync(It.IsAny<RegisterViewModel>()))
            .ReturnsAsync(new ResultMessage<AccountToken> { Success = false, Message = "Auth_EmailTaken" })
            .ReturnsAsync(new ResultMessage<AccountToken> { Success = true });

        Assert.IsType<ViewResult>(screen.Controller.Register());

        screen.Controller.ModelState.AddModelError("Email", "required");
        Assert.IsType<ViewResult>(await screen.Controller.Register(new RegisterViewModel()));
        screen.Service.Verify(a => a.RegisterAsync(It.IsAny<RegisterViewModel>()), Times.Never);
        screen.Controller.ModelState.Clear();

        var refused = await screen.Controller.Register(new RegisterViewModel());
        Assert.IsType<ViewResult>(refused);
        Assert.Contains(screen.Controller.ModelState[string.Empty].Errors, e => e.ErrorMessage == "Auth_EmailTaken");

        // the refusal left an error on the form, a new post starts clean
        screen.Controller.ModelState.Clear();
        var saved = await screen.Controller.Register(new RegisterViewModel());
        Assert.Equal("SignIn", Assert.IsType<RedirectToActionResult>(saved).ActionName);
    }

    // ---------------------------------------------------------------- sign in and out

    [Fact]
    public void SignIn_Get_RemembersWhereToGoBack()
    {
        var screen = new Screen();

        var result = screen.Controller.SignIn("/Order/Upsert");

        Assert.IsType<ViewResult>(result);
        Assert.Equal("/Order/Upsert", screen.Controller.ViewBag.ReturnUrl);
    }

    [Fact]
    public async Task SignIn_InvalidForm_ShowsItAgain_WithoutCallingTheService()
    {
        var screen = new Screen();
        screen.Controller.ModelState.AddModelError("Email", "required");

        Assert.IsType<ViewResult>(await screen.Controller.SignIn(new SignInViewModel(), null));
        screen.Service.Verify(a => a.SignInAsync(It.IsAny<SignInViewModel>()), Times.Never);
    }

    [Theory]
    [InlineData("Auth_ConfirmFirst", true)]
    [InlineData("Auth_BadLogin", false)]
    public async Task SignIn_Refused_ShowsTheMessage_AndOffersToResendOnlyWhenTheEmailIsNotConfirmed(string message, bool offersResend)
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.SignInAsync(It.IsAny<SignInViewModel>())).ReturnsAsync(new ResultMessage<AccountIdentity> { Success = false, Message = message });

        var result = await screen.Controller.SignIn(new SignInViewModel { Email = "a@test.com" }, null);

        Assert.IsType<ViewResult>(result);
        Assert.Contains(screen.Controller.ModelState[string.Empty].Errors, e => e.ErrorMessage == message);
        Assert.Equal(offersResend, screen.Controller.ViewBag.ShowResend);
    }

    [Theory]
    [InlineData("/Order/Upsert", true)]
    [InlineData("https://evil.example/steal", false)]
    [InlineData(null, false)]
    public async Task SignIn_Succeeds_SignsTheCookieIn_AndOnlyFollowsALocalReturnUrl(string returnUrl, bool followsIt)
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.SignInAsync(It.IsAny<SignInViewModel>())).ReturnsAsync(new ResultMessage<AccountIdentity>
        {
            Success = true,
            Data = new AccountIdentity { Id = "u1", UserName = "sara", Email = "sara@test.com", Image = "/img/sara.png", Roles = new List<string> { "Admin", "Staff" } }
        });
        ClaimsPrincipal signedIn = null;
        AuthenticationProperties properties = null;
        screen.Auth.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((_, _, principal, props) => { signedIn = principal; properties = props; })
            .Returns(Task.CompletedTask);

        var result = await screen.Controller.SignIn(new SignInViewModel { Email = "sara@test.com", RememberMe = true }, returnUrl);

        Assert.Equal("u1", signedIn.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("sara", signedIn.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("/img/sara.png", signedIn.FindFirstValue("Image"));
        Assert.Equal(new[] { "Admin", "Staff" }, signedIn.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray());
        Assert.True(properties.IsPersistent);
        Assert.InRange(properties.ExpiresUtc.Value - DateTimeOffset.UtcNow, TimeSpan.FromDays(29), TimeSpan.FromDays(30));
        if (followsIt)
        {
            Assert.Equal(returnUrl, Assert.IsType<LocalRedirectResult>(result).Url);
        }
        else
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Dashboard", redirect.ControllerName);
        }
    }

    [Fact]
    public async Task SignIn_WithoutAnImage_HasNoImageClaim_AndANonPersistentCookie()
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.SignInAsync(It.IsAny<SignInViewModel>())).ReturnsAsync(new ResultMessage<AccountIdentity>
        {
            Success = true,
            Data = new AccountIdentity { Id = "u1", UserName = "sara", Email = "sara@test.com" }
        });
        ClaimsPrincipal signedIn = null;
        AuthenticationProperties properties = null;
        screen.Auth.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>((_, _, principal, props) => { signedIn = principal; properties = props; })
            .Returns(Task.CompletedTask);

        await screen.Controller.SignIn(new SignInViewModel { Email = "sara@test.com" }, null);

        Assert.Null(signedIn.FindFirst("Image"));
        Assert.False(properties.IsPersistent);
        Assert.InRange(properties.ExpiresUtc.Value - DateTimeOffset.UtcNow, TimeSpan.FromHours(11), TimeSpan.FromHours(12));
    }

    [Fact]
    public async Task SignOut_ClearsTheCookie_AndGoesToSignIn()
    {
        var screen = new Screen();

        var result = await screen.Controller.SignOut();

        Assert.Equal("SignIn", Assert.IsType<RedirectToActionResult>(result).ActionName);
        screen.Auth.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    [Fact]
    public void AdminDash_GoesToTheDashboard()
    {
        var result = new Screen().Controller.AdminDash();

        Assert.Equal("Dashboard", Assert.IsType<RedirectToActionResult>(result).ControllerName);
    }

    // ---------------------------------------------------------------- change password

    [Fact]
    public async Task ChangePassword_ShowsTheForm_InvalidOrRefusedShowsItAgain_SuccessSetsTheFlagAndComesBack()
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.ChangePasswordAsync("user-1", It.Is<ChangePasswordViewModel>(m => m.CurrentPassword == "bad"))).ReturnsAsync(new ResultMessage { Success = false, Message = "Auth_WrongPassword" });
        screen.Service.Setup(a => a.ChangePasswordAsync("user-1", It.Is<ChangePasswordViewModel>(m => m.CurrentPassword == "good"))).ReturnsAsync(new ResultMessage { Success = true });

        Assert.IsType<ViewResult>(screen.Controller.ChangePassword());

        screen.Controller.ModelState.AddModelError("NewPassword", "required");
        Assert.IsType<ViewResult>(await screen.Controller.ChangePassword(new ChangePasswordViewModel()));
        screen.Controller.ModelState.Clear();

        Assert.IsType<ViewResult>(await screen.Controller.ChangePassword(new ChangePasswordViewModel { CurrentPassword = "bad" }));
        Assert.Contains(screen.Controller.ModelState[string.Empty].Errors, e => e.ErrorMessage == "Auth_WrongPassword");

        screen.Controller.ModelState.Clear();
        var saved = await screen.Controller.ChangePassword(new ChangePasswordViewModel { CurrentPassword = "good" });
        Assert.Equal("ChangePassword", Assert.IsType<RedirectToActionResult>(saved).ActionName);
        Assert.Equal(true, screen.Controller.TempData["PasswordChanged"]);
    }

    // ---------------------------------------------------------------- e-mail confirmation

    [Fact]
    public void CheckYourInbox_ShowsWhichMailWasSent()
    {
        var screen = new Screen();

        Assert.IsType<ViewResult>(screen.Controller.CheckYourInbox("reset"));
        Assert.Equal("reset", screen.Controller.ViewBag.Type);
    }

    [Fact]
    public async Task ConfirmEmail_AMangledTokenIsAFailure_NotAnException_AndAGoodOneAsksTheService()
    {
        var screen = new Screen();
        var good = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("real-token"));
        screen.Service.Setup(a => a.ConfirmEmailAsync("u1", "real-token")).ReturnsAsync(new ResultMessage { Success = true });
        screen.Service.Setup(a => a.ConfirmEmailAsync("u2", It.IsAny<string>())).ReturnsAsync(new ResultMessage { Success = false });
        screen.Service.Setup(a => a.ConfirmEmailAsync("u1", string.Empty)).ReturnsAsync(new ResultMessage { Success = false });

        // a controller reuses one ViewData for every View(...), so each model is read right after its own call
        var mangled = Assert.IsType<ViewResult>(await screen.Controller.ConfirmEmail("u1", "%%%not-base64%%%")).Model;
        var empty = Assert.IsType<ViewResult>(await screen.Controller.ConfirmEmail("u1", null)).Model;
        var confirmed = Assert.IsType<ViewResult>(await screen.Controller.ConfirmEmail("u1", good)).Model;
        var refused = Assert.IsType<ViewResult>(await screen.Controller.ConfirmEmail("u2", good)).Model;

        Assert.Equal(false, mangled);
        Assert.Equal(false, empty);
        Assert.Equal(true, confirmed);
        Assert.Equal(false, refused);
    }

    [Fact]
    public async Task ResendConfirmation_MailsALinkOnlyForAnAccountThatNeedsOne_AndAlwaysAnswersTheSame()
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.GetConfirmationTokenAsync("known@test.com")).ReturnsAsync(new AccountToken { UserId = "u1", Email = "known@test.com", FirstName = "Sara", Token = "abc" });

        var unknown = await screen.Controller.ResendConfirmation("nobody@test.com");
        var known = await screen.Controller.ResendConfirmation("known@test.com");

        Assert.Equal("CheckYourInbox", Assert.IsType<RedirectToActionResult>(unknown).ActionName);
        Assert.Equal("CheckYourInbox", Assert.IsType<RedirectToActionResult>(known).ActionName);
        screen.Email.Verify(a => a.SendAsync("known@test.com", "Confirm your email", It.Is<string>(body => body.Contains("https://app/link") && body.Contains("Sara"))), Times.Once);
        screen.Email.Verify(a => a.SendAsync("nobody@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ---------------------------------------------------------------- forgotten and reset password

    [Fact]
    public async Task ForgetPassword_InvalidShowsTheFormAgain_UnknownEmailMailsNothing_AKnownOneGetsTheLink()
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.GetPasswordResetTokenAsync("known@test.com")).ReturnsAsync(new AccountToken { Email = "known@test.com", Token = "t" });

        Assert.IsType<ViewResult>(screen.Controller.ForgetPassword());
        screen.Controller.ModelState.AddModelError("Email", "required");
        Assert.IsType<ViewResult>(await screen.Controller.ForgetPassword(new ForgetPasswordViewModel()));
        screen.Controller.ModelState.Clear();

        var unknown = await screen.Controller.ForgetPassword(new ForgetPasswordViewModel { Email = "nobody@test.com" });
        var known = await screen.Controller.ForgetPassword(new ForgetPasswordViewModel { Email = "known@test.com" });

        Assert.Equal("reset", ((RedirectToActionResult)unknown).RouteValues["type"]);
        Assert.Equal("reset", ((RedirectToActionResult)known).RouteValues["type"]);
        screen.Email.Verify(a => a.SendAsync("known@test.com", "Reset your password", It.Is<string>(body => body.Contains("https://app/link"))), Times.Once);
        screen.Email.Verify(a => a.SendAsync("nobody@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ResetPassword_Get_CarriesTheEmailAndTokenOfTheLink()
    {
        var screen = new Screen();

        var result = screen.Controller.ResetPassword("a@test.com", "tok");

        var model = Assert.IsType<ResetPasswordViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("a@test.com", model.Email);
        Assert.Equal("tok", model.Token);
    }

    [Fact]
    public async Task ResetPassword_InvalidOrRefusedShowsItAgain_AnUnknownAccountLooksLikeSuccess_SuccessGoesToSignIn()
    {
        var screen = new Screen();
        screen.Service.Setup(a => a.ResetPasswordAsync(It.Is<ResetPasswordViewModel>(m => m.Email == "bad@test.com"))).ReturnsAsync(new ResultMessage { Success = false, Message = "Auth_InvalidToken" });
        screen.Service.Setup(a => a.ResetPasswordAsync(It.Is<ResetPasswordViewModel>(m => m.Email == "ghost@test.com"))).ReturnsAsync(new ResultMessage { Success = false, Message = "RecordNotFound" });
        screen.Service.Setup(a => a.ResetPasswordAsync(It.Is<ResetPasswordViewModel>(m => m.Email == "ok@test.com"))).ReturnsAsync(new ResultMessage { Success = true });

        screen.Controller.ModelState.AddModelError("Password", "required");
        Assert.IsType<ViewResult>(await screen.Controller.ResetPassword(new ResetPasswordViewModel()));
        screen.Controller.ModelState.Clear();

        var refused = await screen.Controller.ResetPassword(new ResetPasswordViewModel { Email = "bad@test.com" });
        Assert.IsType<ViewResult>(refused);
        Assert.Contains(screen.Controller.ModelState[string.Empty].Errors, e => e.ErrorMessage == "Auth_InvalidToken");

        // the screen must not reveal whether an e-mail has an account
        screen.Controller.ModelState.Clear();
        Assert.Equal("SignIn", Assert.IsType<RedirectToActionResult>(await screen.Controller.ResetPassword(new ResetPasswordViewModel { Email = "ghost@test.com" })).ActionName);
        Assert.Equal("SignIn", Assert.IsType<RedirectToActionResult>(await screen.Controller.ResetPassword(new ResetPasswordViewModel { Email = "ok@test.com" })).ActionName);
    }
}
