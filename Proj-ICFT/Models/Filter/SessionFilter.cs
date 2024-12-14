using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Proj_ICFT.Models.Filter
{
    public class SessionFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            base.OnActionExecuting(context);
            if (context.HttpContext.Session.GetString("_UserToken") == null || context.HttpContext.Session.GetString("_UserToken") == "")
                ModifyResult(context);


            // Do something before the action executes.
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (context.HttpContext.Response.StatusCode.Equals(401))
                ModifyResult(context);

            // Do something after the action executes.
        }

        private void ModifyResult(dynamic context)
        {
            bool isAjax = ((HttpContext)context.HttpContext).Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                context.Result = new JsonResult(new { SessionTimeout = true });
            }
            else
                context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Account" }, { "action", "Login" } });

            ((Controller)context.Controller).TempData.Clear();
            ((Controller)context.Controller).TempData["ErrorMessage"] = "É necessario estar logado para acessar essa página";
        }


    }
}
