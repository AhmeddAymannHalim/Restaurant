using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ResturantPL.Helper
{
    public class RedirectToSignInFilter : IActionFilter
    {
        private readonly string[] _allowedRoles;

        public RedirectToSignInFilter(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                // If user is not authenticated, redirect to sign-in page
                context.Result = new RedirectToActionResult("SignIn", "Account", null);
            }
            else if (!_allowedRoles.Any(role => user.IsInRole(role)))
            {
                // If user is authenticated but not in any allowed roles, redirect to sign-in page
                context.Result = new RedirectToActionResult("SignIn", "Account", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Not needed
        }
    }

}
