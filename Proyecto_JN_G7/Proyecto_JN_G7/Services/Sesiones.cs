using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Proyecto_JN_G7.Services
{
    public class Sesiones : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Si no hay un JWT, que nos saque al index
            if (context.HttpContext.Session.GetString("JWT") == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
