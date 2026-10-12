using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ResturantPL.Filters
{
    public class AjaxRedirectFilter : IAlwaysRunResultFilter
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AjaxRedirectFilter(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.ContainsKey("X-Ajax-Form"))
            {
                return;
            }

            var url = context.Result switch
            {
                RedirectToActionResult redirect => _urlHelperFactory.GetUrlHelper(context).Action(redirect.ActionName,
                    redirect.ControllerName,
                    redirect.RouteValues),
                RedirectResult redirect => redirect.Url,
                LocalRedirectResult redirect => redirect.Url,
                _ => null
            };

            if (url is not null)
            {
                context.Result = new JsonResult(new { redirect = url });
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
