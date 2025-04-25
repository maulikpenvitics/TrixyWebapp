using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrixyWebapp.Filters
{
    public class SessionCheckAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session.GetString("UserId");
            if (session == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                { "controller", "Account" },
                { "action", "Login" }
            });
            }
        }
    }
}
