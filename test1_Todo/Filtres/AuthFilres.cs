using Microsoft.AspNetCore.Mvc.Filters;

namespace test1_Todo.Filtres
{
    public class AuthFilres : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            var isConnected = context.HttpContext.Session.GetString("IsConnected");
            if (string.IsNullOrEmpty(isConnected) || isConnected != "True")
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Inscription", "Auth", null);
            }
        }
    }
}